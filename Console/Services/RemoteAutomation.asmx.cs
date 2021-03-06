﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Web.Services;
using Cognifide.PowerShell.PowerShellIntegrations.Host;
using Cognifide.PowerShell.PowerShellIntegrations.Settings;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources.Media;
using Sitecore.Security.Authentication;

namespace Cognifide.PowerShell.Console.Services
{
    /// <summary>
    /// Summary description for RemoteAutomation:
    /// The service is used by to execute scripts blocks from remote locations
    /// for the purpose of BDD tests and remote integration with Windows PowerShell.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class RemoteAutomation : WebService
    {

        private void Login(string userName, string password)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                if (!userName.Contains("\\"))
                {
                    userName = "sitecore\\" + userName;
                }
                bool loggedIn = AuthenticationManager.Login(userName, password, false);
                if (!loggedIn)
                {
                    throw new AuthenticationException("Unrecognized user or password mismatch.");
                }
            }
        }

        [WebMethod]
        public NameValue[] ExecuteScript(string userName, string password, string script, string returnVariables)
        {
            Login(userName, password);

            using (var scriptSession = new ScriptSession(ApplicationNames.RemoteAutomation, false))
            {
                scriptSession.ExecuteScriptPart(scriptSession.Settings.Prescript);
                scriptSession.ExecuteScriptPart(script);

                var result = new List<NameValue>();

                if (scriptSession.Output.Count > 0)
                {
                    result.Add(new NameValue
                    {
                        Name = "output",
                        Value =
                            scriptSession.Output.Select(p => p.Terminated ? p.Text + "\n" : p.Text).Aggregate(
                                (current, next) => current + next)
                    });
                }
                result.AddRange(
                    returnVariables.Split('|').Select(variable => new NameValue
                    {
                        Name = variable,
                        Value = (scriptSession.GetVariable(variable) ?? string.Empty).ToString()
                    }));
                return result.ToArray();
            }
        }

        [WebMethod]
        public string ExecuteScriptBlock(string userName, string password, string script, string cliXmlArgs)
        {
            Login(userName, password);

            using (var scriptSession = new ScriptSession(ApplicationNames.RemoteAutomation, false))
            {
                scriptSession.SetVariable("cliXmlArgs", cliXmlArgs);
                scriptSession.ExecuteScriptPart(scriptSession.Settings.Prescript);
                scriptSession.ExecuteScriptPart("$params = ConvertFrom-CliXml -InputObject $cliXmlArgs");
                scriptSession.ExecuteScriptPart(script + "| ConvertTo-CliXml");

                return scriptSession.Output.Select(p => p.Terminated ? p.Text + "\n" : p.Text).Aggregate(
                                (current, next) => current + next);
            }
        }

        [WebMethod]
        public bool UploadFile(string userName, string password, string filePath, byte[] fileContent, string database, string language)
        {
            try
            {
                Login(userName, password);

                string dirName = (Path.GetDirectoryName(filePath) ?? string.Empty).Replace('\\', '/');
                if (!dirName.StartsWith(Constants.MediaLibraryPath))
                {
                    dirName = Constants.MediaLibraryPath + (dirName.StartsWith("/") ? dirName : "/" + dirName);
                }

                var mco = new MediaCreatorOptions();
                mco.Database = Factory.GetDatabase(database);
                mco.Language = Language.Parse(language);
                mco.Versioned = Settings.Media.UploadAsVersionableByDefault;
                mco.Destination = string.Format("{0}/{1}", dirName, Path.GetFileNameWithoutExtension(filePath));

                var mc = new MediaCreator();
                using (MemoryStream stream = new MemoryStream(fileContent))
                {
                    mc.CreateFromStream(stream, Path.GetFileName(filePath), mco);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error during uploading file using PowerShell web service", ex);
                return false;
            }
            return true;
        }

        [WebMethod]
        public byte[] DownloadFile(string userName, string password, string filePath, string database, string language)
        {
            try
            {
                Login(userName, password);

                string dirName = (Path.GetDirectoryName(filePath) ?? string.Empty).Replace('\\', '/');
                if (!dirName.StartsWith(Constants.MediaLibraryPath))
                {
                    dirName = Constants.MediaLibraryPath + (dirName.StartsWith("/") ? dirName : "/" + dirName);
                }
                var itemname = dirName + "/" + Path.GetFileNameWithoutExtension(filePath);
                var db = Factory.GetDatabase(database);
                var item = (MediaItem)db.GetItem(itemname);
                using (var stream = item.GetMediaStream())
                {
                    var result = new byte[stream.Length];
                    stream.Read(result, 0, (int)stream.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error during uploading file using PowerShell web service", ex);
                return new byte[0];
            }
        }

        public class NameValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}