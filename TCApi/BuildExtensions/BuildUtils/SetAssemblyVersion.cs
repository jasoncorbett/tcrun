using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildUtils
{

	public class SetAssemblyVersion : Task
	{
		[Required]
		public string Version { get; set; }
		
		public override bool Execute ()
		{
			Regex CommentedLine = new Regex ("^\\s*//.*$");
			Regex AssemblyVersionString = new Regex ("\".*?\"");
			string assemblyinfo = Path.Combine ("Properties", "AssemblyInfo.cs");
			if (!File.Exists (assemblyinfo))
			{
				Log.LogError ("AssemblyInfo.cs not found in Properties folder, aborting.", null);
				return false;
			}
			
			string[] contents = File.ReadAllLines (assemblyinfo);
			for (int i = 0; i < contents.Length; i++)
			{
				string line = contents[i];
				if (CommentedLine.Match (line).Success)
				{
					continue;
				}
				
				if (line.StartsWith ("[assembly: AssemblyInformationalVersion"))
				{
					contents[i] = AssemblyVersionString.Replace (line, "\"" + Version + "\"");
					Log.LogMessage ("Replacing line '{0}' with line '{1}'.", new object[] { line, contents[i] });
				}
			}
			
			File.WriteAllLines (assemblyinfo, contents);
			return true;
		}

	}
}
