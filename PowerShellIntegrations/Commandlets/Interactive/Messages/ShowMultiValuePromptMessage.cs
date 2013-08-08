﻿using System;
using System.Text;
using System.Web;
using Cognifide.PowerShell.Console.Services;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Jobs.AsyncUI;
using Sitecore.Resources.Media;
using Sitecore.Shell.Framework;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Cognifide.PowerShell.PowerShellIntegrations.Commandlets.Interactive.Messages
{
    [Serializable]
    public class ShowMultiValuePromptMessage : IMessage
    {

        public object[] Parameters { get; private set; }
        public string Width { get; private set; }
        public string Height { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        private Handle jobHandle;

        public ShowMultiValuePromptMessage(object[] parameters, string width, string height, string title, string description)
        {
            jobHandle = JobContext.JobHandle;
            Parameters = parameters;
            Width = width;
            Height = height;
            Title = title;
            Description = description;
        }

        /// <summary>
        /// Shows a confirmation dialog.
        /// 
        /// </summary>
        protected virtual void ShowUI()
        {
            string resultSig = Guid.NewGuid().ToString();
            HttpContext.Current.Session[resultSig] = Parameters;
            UrlString urlString = new UrlString(UIUtil.GetUri("control:PowerShellMultiValuePrompt"));
            urlString.Add("sid", resultSig);
            urlString.Add("te", Title);
            urlString.Add("ds", Description);
            SheerResponse.ShowModalDialog(urlString.ToString(), Width, Height, "", true);
        }

        /// <summary>
        /// Starts the pipeline.
        /// 
        /// </summary>
        public void Execute()
        {
            Context.ClientPage.Start(this, "Pipeline");
        }

        /// <summary>
        /// Entry point for a pipeline.
        /// 
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Pipeline(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                Context.ClientPage.ServerProperties["#pipelineJob"] = jobHandle.ToString();
                ShowUI();
                args.WaitForPostBack();
            }
            else
            {
                jobHandle = Handle.Parse(StringUtil.GetString(Context.ClientPage.ServerProperties["#pipelineJob"]));
                Job job = JobManager.GetJob(this.jobHandle);
                if (job == null)
                    return;
                if (args.HasResult)
                {
                    var result = HttpContext.Current.Session[args.Result];
                    HttpContext.Current.Session.Remove(args.Result);
                    job.MessageQueue.PutResult(result);
                }
                else
                    job.MessageQueue.PutResult(null);
            }
        }

    }
}