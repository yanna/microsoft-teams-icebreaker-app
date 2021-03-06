//----------------------------------------------------------------------------------------------
// <copyright file="WelcomeTeamAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
namespace Icebreaker.Helpers.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Web.Hosting;
    using Icebreaker.Properties;
    using Microsoft.Azure;

    /// <summary>
    /// Builder class for the team welcome message
    /// </summary>
    public class WelcomeTeamAdaptiveCard
    {
        private static readonly string CardTemplate;

        static WelcomeTeamAdaptiveCard()
        {
            var cardJsonFilePath = HostingEnvironment.MapPath("~/Helpers/AdaptiveCards/WelcomeTeamAdaptiveCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// Creates the adaptive card for the team welcome message
        /// </summary>
        /// <param name="teamName">The team name</param>
        /// <param name="teamId">Team id</param>
        /// <param name="botChannelAccountId">Bot id that will allow deeplink to chat with the bot</param>
        /// <param name="botInstaller">The name of the person that installed the bot</param>
        /// <returns>The welcome team adaptive card</returns>
        public static string GetCardJson(string teamName, string teamId, string botChannelAccountId, string botInstaller)
        {
            string teamIntroPart1;
            if (string.IsNullOrEmpty(botInstaller))
            {
                teamIntroPart1 = string.Format(Resources.InstallMessageUnknownInstaller, teamName);
            }
            else
            {
                teamIntroPart1 = string.Format(Resources.InstallMessageKnownInstaller, botInstaller, teamName);
            }

            string teamIntroPart2 = Resources.InstallMessageBotDescription;
            string teamIntroPart3 = Resources.InstallMessageInstruction;
            var suggestedNextStep = Resources.WelcomeTeamSuggestedNextStep;

            var baseDomain = CloudConfigurationManager.GetSetting("AppBaseDomain");
            var welcomeCardImageUrl = $"https://{baseDomain}/Content/welcome-card-image.png";
            var salutationText = Resources.SalutationTitleText;
            var chatWithMeButtonText = Resources.ChatWithMeButtonText;

            var variablesToValues = new Dictionary<string, string>()
            {
                { "teamIntroPart1", teamIntroPart1 },
                { "teamIntroPart2", teamIntroPart2 },
                { "teamIntroPart3", teamIntroPart3 },
                { "suggestedNextStep", suggestedNextStep },
                { "welcomeCardImageUrl", welcomeCardImageUrl },
                { "salutationText", salutationText },
                { "chatWithMeButtonText", chatWithMeButtonText },
                { "botChatId", botChannelAccountId },
                { "botMessage", AdaptiveCardHelper.GetChatWithMeMessage(teamId) }
            };

            var cardBody = CardTemplate;
            foreach (var kvp in variablesToValues)
            {
                cardBody = cardBody.Replace($"%{kvp.Key}%", kvp.Value);
            }

            return cardBody;
        }
    }
}