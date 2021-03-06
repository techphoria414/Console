﻿using System.Management.Automation;
using System.Web;

namespace Cognifide.PowerShell.PowerShellIntegrations.Commandlets.Session
{
    [Cmdlet("Restart", "Application")]
    public class RestartApplication : BaseCommand
    {
        protected override void BeginProcessing()
        {
            HttpRuntime.UnloadAppDomain();
        }
    }
}