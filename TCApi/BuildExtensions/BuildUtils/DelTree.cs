using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildUtils
{
	public class DelTree: Task
	{
		[Required]
		public String Directories { get; set; }

		public override bool Execute ()
		{
			String[] dirs = Directories.Split (';');
			foreach (string dir in dirs)
			{
				if (Directory.Exists (dir))
				{
					Log.LogMessage ("Removing Directory [{0}]", new object[] { dir });
					Directory.Delete (dir, true);
				} else
				{
					Log.LogMessage ("Directory does not exist, skipping [{0}]", new object[] { dir });
				}
			}
			return true;
		}
	}
}
