﻿using System;
using System.IO;
using System.Management.Automation;
using Sitecore.Install.Files;
using Sitecore.IO;

namespace Cognifide.PowerShell.PowerShellIntegrations.Commandlets.Packages
{
    [Cmdlet("New", "ExplicitFileSource")]
    [OutputType(new[] {typeof (ExplicitFileSource)})]
    public class NewExplicitFileSourceCommand : BasePackageCommand
    {
        private ExplicitFileSource source;

        [Parameter(Position = 0, Mandatory = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public FileSystemInfo File { get; set; }

        protected override void BeginProcessing()
        {
            source = new ExplicitFileSource {Name = Name};
        }

        protected override void ProcessRecord()
        {
            if (File is FileInfo)
            {
                string siteRoot = FileUtil.MapPath("/");
                string fullName = (File as FileInfo).FullName;
                if (fullName.StartsWith(siteRoot, StringComparison.OrdinalIgnoreCase))
                {
                    fullName = fullName.Substring(siteRoot.Length - 1).Replace('\\', '/');
                }
                source.Entries.Add(fullName);
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(source, false);
        }
    }
}