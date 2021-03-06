﻿//----------------------------------------------------------------------------------------------
// <copyright file="ChooseTeamHeroCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Icebreaker.Helpers.HeroCards
{
    using System.Collections.Generic;
    using System.Linq;
    using Icebreaker.Controllers;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;

    /// <summary>
    /// Choose team hero card. Presents buttons for all teams.
    /// </summary>
    public static class ChooseTeamHeroCard
    {
        /// <summary>
        /// Show a list of teams to pick for a particular action.
        /// </summary>
        /// <param name="text">Text for the team prompt</param>
        /// <param name="teams">List of teams to choose from</param>
        /// <param name="actionMessage">The action message</param>
        /// <returns>Choose team card</returns>
        public static HeroCard GetCard(string text, List<TeamContext> teams, string actionMessage)
        {
            var teamActions = new List<CardAction>();
            var orderedByName = teams.OrderBy(t => t.TeamName).ToList();
            foreach (var team in orderedByName)
            {
                var teamName = team.TeamName;

                var teamCardAction = new CardAction()
                {
                    Title = teamName,
                    DisplayText = teamName,
                    Type = ActionTypes.MessageBack,
                    Text = actionMessage,
                    Value = JsonConvert.SerializeObject(team)
                };
                teamActions.Add(teamCardAction);
            }

            var card = new HeroCard()
            {
                Text = text,
                Buttons = teamActions
            };

            return card;
        }
    }
}