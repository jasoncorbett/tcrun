using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildUtils
{

	public class GenerateVersionNumber : Task
	{
		[Required]
		public string Major { get; set; }
		
		[Required]
		public string Minor { get; set; }

		[Required]
		public string Release { get; set; }
		
		[Required]
		public string BuildNumber { get; set; }
		
		[Output]
		public string Version { get; set; }
		
		public override bool Execute ()
		{
			if (BuildNumber == "AUTO")
			{
				if (Environment.GetEnvironmentVariable ("BUILD_NUMBER") != null)
				{
					BuildNumber = Environment.GetEnvironmentVariable ("BUILD_NUMBER");
				} else
				{
					BuildNumber = "SNAPSHOT-" + ((int)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds).ToString ();
				}
			}
			
			Version = Major + "." + Minor + "." + Release + "." + BuildNumber;
			Log.LogMessage ("Version for build generated as '{0}'.", new object[] { Version });
			return true;
		}

	}
}
