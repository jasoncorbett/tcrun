using System;
using System.IO;
using Ionic.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildUtils
{

	public class CreateZip : Task
	{
		[Required]
		public string Directory { get; set; }
		
		public string ZipFileName { get; set; }

		public override bool Execute ()
		{
			if (ZipFileName == null || ZipFileName == String.Empty)
			{
				ZipFileName = Directory + ".zip";
			}
			
			using (ZipFile zip = new ZipFile (ZipFileName))
			{
				Log.LogMessage ("Adding Directory '{0}' to zip file '{1}'.", new object[] { Directory, ZipFileName });
				zip.AddDirectory (Directory, Path.GetFileNameWithoutExtension(ZipFileName));
				zip.Save ();
			}
			return true;
		}

	}
}
