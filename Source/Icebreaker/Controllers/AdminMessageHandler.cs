﻿//----------------------------------------------------------------------------------------------
// <copyright file="AdminMessageHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Icebreaker.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.UI.WebControls;
    using Icebreaker.Helpers;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using Newtonsoft.Json;
    using Properties;

    /// <summary>
    /// Handles admin messages
    /// </summary>
    public class AdminMessageHandler
    {
        private readonly IcebreakerBot bot;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminMessageHandler"/> class.
        /// </summary>
        /// <param name="bot">The Icebreaker bot instance</param>
        /// <param name="telemetryClient">The telemetry client instance</param>
        public AdminMessageHandler(IcebreakerBot bot, TelemetryClient telemetryClient)
        {
            this.bot = bot;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Whether this handler will handle the message
        /// </summary>
        /// <param name="msgId">Message id</param>
        /// <returns>bool</returns>
        public bool CanHandleMessage(string msgId)
        {
            var acceptedMsgs = new List<string>
            {
                MessageIds.AdminMakePairs,
                MessageIds.AdminNotifyPairs,
                MessageIds.AdminChangeNotifyModeNeedApproval,
                MessageIds.AdminChangeNotifyModeNoApproval,
                MessageIds.AdminEditTeamSettings
            };
            return acceptedMsgs.Contains(msgId.ToLowerInvariant());
        }

        /// <summary>
        /// Handle the incoming message
        /// </summary>
        /// <param name="msgId">message id</param>
        /// <param name="connectorClient">connector client</param>
        /// <param name="activity">activity that had the message</param>
        /// <param name="senderAadId">sender AAD id</param>
        /// <param name="senderChannelAccountId">sender ChannelAccount id</param>
        /// <returns>Task</returns>
        public async Task HandleMessage(string msgId, ConnectorClient connectorClient, Activity activity, string senderAadId, string senderChannelAccountId)
        {
            if (msgId == MessageIds.AdminMakePairs)
            {
                await this.HandleAdminMakePairs(connectorClient, activity, senderAadId, senderChannelAccountId);
            }
            else if (msgId == MessageIds.AdminNotifyPairs)
            {
                if (activity.Value != null && activity.Value.ToString().TryParseJson(out MakePairsResult result))
                {
                    await this.HandleAdminNotifyPairs(connectorClient, activity, senderAadId, result.TeamId);
                }
            }
            else if (msgId == MessageIds.AdminChangeNotifyModeNeedApproval)
            {
                await this.HandleAdminNotifyNeedApproval(connectorClient, activity, senderAadId, senderChannelAccountId);
            }
            else if (msgId == MessageIds.AdminChangeNotifyModeNoApproval)
            {
                await this.HandleAdminNotifyNoApproval(connectorClient, activity, senderAadId, senderChannelAccountId);
            }
            else if (msgId == MessageIds.AdminEditTeamSettings)
            {
                await this.HandleAdminEditTeamSettings(connectorClient, activity, senderAadId, senderChannelAccountId);
            }
        }

        private async Task HandleAdminMakePairs(ConnectorClient connectorClient, Activity activity, string senderAadId, string senderChannelAccountId)
        {
            if (activity.Value != null && activity.Value.ToString().TryParseJson(out TeamContext request))
            {
                var team = await this.bot.GetInstalledTeam(request.TeamId);
                await this.HandleAdminMakePairsForTeam(connectorClient, activity, senderAadId, team, request.TeamName);
            }
            else
            {
                await this.HandleAdminActionWithNoTeamSpecified(
                    connectorClient,
                    activity,
                    senderAadId,
                    senderChannelAccountId,
                    adminActionName: Resources.AdminActionGeneratePairs,
                    adminActionMessageId: MessageIds.AdminMakePairs,
                    this.HandleAdminMakePairsForTeam);
            }
        }

        private async Task HandleAdminNotifyNoApproval(ConnectorClient connectorClient, Activity activity, string senderAadId, string senderChannelAccountId)
        {
            if (activity.Value != null && activity.Value.ToString().TryParseJson(out TeamContext request))
            {
                var team = await this.bot.GetInstalledTeam(request.TeamId);
                await this.HandleAdminNotifyNoApprovalForTeam(connectorClient, activity, senderAadId, team, request.TeamName);
            }
            else
            {
                await this.HandleAdminActionWithNoTeamSpecified(
                    connectorClient,
                    activity,
                    senderAadId,
                    senderChannelAccountId,
                    adminActionName: Resources.AdminActionNotifyNoApproval,
                    adminActionMessageId: MessageIds.AdminChangeNotifyModeNoApproval,
                    this.HandleAdminNotifyNoApprovalForTeam);
            }
        }

        private async Task HandleAdminNotifyNeedApproval(ConnectorClient connectorClient, Activity activity, string senderAadId, string senderChannelAccountId)
        {
            if (activity.Value != null && activity.Value.ToString().TryParseJson(out TeamContext request))
            {
                var team = await this.bot.GetInstalledTeam(request.TeamId);
                await this.HandleAdminNotifyNeedApprovalForTeam(connectorClient, activity, senderAadId, team, request.TeamName);
            }
            else
            {
                await this.HandleAdminActionWithNoTeamSpecified(
                    connectorClient,
                    activity,
                    senderAadId,
                    senderChannelAccountId,
                    adminActionName: Resources.AdminActionNotifyNeedApproval,
                    adminActionMessageId: MessageIds.AdminChangeNotifyModeNeedApproval,
                    this.HandleAdminNotifyNeedApprovalForTeam);
            }
        }

        private async Task HandleAdminNotifyNoApprovalForTeam(ConnectorClient connectorClient, Activity activity, string senderAadId, TeamInstallInfo team, string teamName)
        {
            var isSuccess = await this.bot.ChangeTeamNotifyPairsMode(needApproval: false, team);

            Activity reply = activity.CreateReply();
            reply.Text = isSuccess ? Resources.NotifyModeNoApprovalSuccess : Resources.NotifyModeFail;
            await connectorClient.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task HandleAdminNotifyNeedApprovalForTeam(ConnectorClient connectorClient, Activity activity, string senderAadId, TeamInstallInfo team, string teamName)
        {
            var isSuccess = await this.bot.ChangeTeamNotifyPairsMode(needApproval: true, team);

            Activity reply = activity.CreateReply();
            reply.Text = isSuccess ? Resources.NotifyModeNeedApprovalSuccess : Resources.NotifyModeFail;
            await connectorClient.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task HandleAdminActionWithNoTeamSpecified(
            ConnectorClient connectorClient,
            Activity activity,
            string senderAadId,
            string senderChannelAccountId,
            string adminActionName,
            string adminActionMessageId,
            Func<ConnectorClient, Activity, string, TeamInstallInfo, string, Task> adminActionFcn)
        {
            this.telemetryClient.TrackTrace($"User {senderAadId} triggered {adminActionName} with no team specified");

            var teamsAllowingAdminActionsByUser = await this.bot.GetTeamsAllowingAdminActionsByUser(senderChannelAccountId);

            if (teamsAllowingAdminActionsByUser.Count == 0)
            {
                var noTeamMsg = string.Format(Resources.AdminActionNoTeamMsg, adminActionName);
                var noTeamReply = activity.CreateReply(noTeamMsg);
                await connectorClient.Conversations.ReplyToActivityAsync(noTeamReply);
            }
            else if (teamsAllowingAdminActionsByUser.Count == 1)
            {
                var team = teamsAllowingAdminActionsByUser.First();
                var teamName = await this.bot.GetTeamNameAsync(connectorClient, team.Id);
                await adminActionFcn.Invoke(connectorClient, activity, senderAadId, team, teamName);
            }
            else
            {
                var teamActions = new List<CardAction>();

                foreach (var team in teamsAllowingAdminActionsByUser)
                {
                    var teamName = await this.bot.GetTeamNameAsync(connectorClient, team.Id);
                    var teamCardAction = new CardAction()
                    {
                        Title = teamName,
                        DisplayText = teamName,
                        Type = ActionTypes.MessageBack,
                        Text = adminActionMessageId,
                        Value = JsonConvert.SerializeObject(new TeamContext { TeamId = team.Id, TeamName = teamName })
                    };
                    teamActions.Add(teamCardAction);
                }

                var pickTeamReply = activity.CreateReply();
                pickTeamReply.Attachments = new List<Attachment>
                {
                    new HeroCard()
                    {
                        Text = string.Format(Resources.AdminActionWhichTeamText, adminActionName),
                        Buttons = teamActions
                    }.ToAttachment(),
                };

                await connectorClient.Conversations.ReplyToActivityAsync(pickTeamReply);
            }
        }

        private async Task HandleAdminMakePairsForTeam(ConnectorClient connectorClient, Activity activity, string senderAadId, TeamInstallInfo team, string teamName)
        {
            this.telemetryClient.TrackTrace($"User {senderAadId} triggered make pairs");

            var matchResult = await this.bot.MakePairsForTeam(team);

            Activity reply = activity.CreateReply();
            reply.Attachments = new List<Attachment>
            {
                this.bot.CreateMatchAttachment(matchResult, team.Id, teamName)
            };
            await connectorClient.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task HandleAdminNotifyPairs(ConnectorClient connectorClient, Activity activity, string senderAadId, string teamId)
        {
            this.telemetryClient.TrackTrace($"User {senderAadId} triggered notify pairs");

            string replyMessage = string.Empty;

            try
            {
                var makePairsResult = JsonConvert.DeserializeObject<MakePairsResult>(activity.Value.ToString());

                var members = await connectorClient.Conversations.GetConversationMembersAsync(teamId);
                var membersByChannelAccountId = members.ToDictionary(key => key.Id, value => value);

                // Evaluate all values so we can fail early if someone no longer exists
                var pairs = makePairsResult.PairChannelAccountIds.Select(pair => new Tuple<ChannelAccount, ChannelAccount>(
                    membersByChannelAccountId[pair.Item1],
                    membersByChannelAccountId[pair.Item2])).ToList();

                var team = await this.bot.GetInstalledTeam(teamId);
                var numPairsNotified = await this.bot.NotifyAllPairs(team, pairs);
                replyMessage = string.Format(Resources.ManualNotifiedUsersMessage, numPairsNotified);
            }
            catch (Exception ex)
            {
                replyMessage = Resources.ManualNotifiedUsersErrorMessage;
                this.telemetryClient.TrackTrace($"Error while notifying pairs: {ex.Message}", SeverityLevel.Warning);
                this.telemetryClient.TrackException(ex);
            }

            Activity reply = activity.CreateReply(replyMessage);
            await connectorClient.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task HandleAdminEditTeamSettings(ConnectorClient connectorClient, Activity activity, string senderAadId, string senderChannelAccountId)
        {
            if (activity.Value != null && activity.Value.ToString().TryParseJson(out TeamContext request))
            {
                var team = await this.bot.GetInstalledTeam(request.TeamId);
                await this.HandleAdminEditTeamSettingsForTeam(connectorClient, activity, senderAadId, team, request.TeamName);
            }
            else
            {
                await this.HandleAdminActionWithNoTeamSpecified(
                    connectorClient,
                    activity,
                    senderAadId,
                    senderChannelAccountId,
                    adminActionName: Resources.AdminActionEditTeamSettings,
                    adminActionMessageId: MessageIds.AdminEditTeamSettings,
                    this.HandleAdminEditTeamSettingsForTeam);
            }
        }

        private Task HandleAdminEditTeamSettingsForTeam(ConnectorClient connectorClient, Activity activity, string senderAadId, TeamInstallInfo team, string teamName)
        {
            var replyActivity = activity.CreateReply();
            return this.bot.EditTeamSettings(connectorClient, replyActivity, team.TeamId, teamName);
        }
    }
}