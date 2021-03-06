﻿using System.Linq;
using System.Web.UI;
using Cognifide.PowerShell.PowerShellIntegrations;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Rules;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;
using Control = Sitecore.Web.UI.HtmlControls.Control;

namespace Cognifide.PowerShell.SitecoreIntegrations.Controls
{
    public class RibbonExportScriptsPanel : RibbonPanel
    {
        public override void Render(HtmlTextWriter output, Ribbon ribbon, Item button, CommandContext context)
        {
            Item parent = Factory.GetDatabase("master").GetItem(ScriptLibrary.Path + "Internal/List View/Export");

            foreach (Item scriptItem in parent.Children)
            {
                if (!EvaluateRules(scriptItem["ShowRule"]))
                {
                    continue;
                }
                RenderSmallButton(output, ribbon, Control.GetUniqueID("export"), Translate.Text(scriptItem.DisplayName),
                    scriptItem["__Icon"], string.Empty,
                    string.Format("export:results(scriptDb={0},scriptID={1})", scriptItem.Database.Name, scriptItem.ID),
                    EvaluateRules(scriptItem["EnableRule"]), false);
            }
        }

        public static bool EvaluateRules(string strRules)
        {
            if (string.IsNullOrEmpty(strRules) || strRules.Length < 20)
            {
                return true;
            }
            // hacking the rules xml
            RuleList<RuleContext> rules = RuleFactory.ParseRules<RuleContext>(Factory.GetDatabase("master"), strRules);
            var ruleContext = new RuleContext();

            return rules.Rules.Any(rule => rule.Evaluate(ruleContext));
        }
    }
}