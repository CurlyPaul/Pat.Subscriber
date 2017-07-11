﻿using System;
using System.Linq;
using log4net;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using PB.ITOps.Messaging.PatLite.SubscriberRules;

namespace PB.ITOps.Messaging.PatLite
{
    public class SubscriptionBuilder
    {
        private readonly ILog _log;
        private readonly SubscriberConfig _config;

        public SubscriptionBuilder(ILog log, SubscriberConfig config)
        {
            _log = log;
            _config = config;
        }

        public void Build(SubscriptionDescription subscriptionDescription, string[] messagesTypes)
        {
            var clientIndex = 1;
            foreach (var connectionString in _config.ConnectionStrings)
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    _log.Info($"Buiding subscription {clientIndex}...");
                    BuildSubscription(connectionString, subscriptionDescription, messagesTypes);
                }
                else
                {
                    _log.Info($"Skipping subscription {clientIndex}, connection string is null or empty");
                }
            }
        }
        
        public SubscriptionDescription CommonSubscriptionDescription()
        {
            return new SubscriptionDescription(_config.TopicName, _config.SubscriberName)
            {
                DefaultMessageTimeToLive = new TimeSpan(30, 0, 0, 0)
            };
        }

        private void BuildSubscription(string connectionString, SubscriptionDescription subscriptionDescription, string[] messagesTypes)
        {
            var topicName = _config.TopicName;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(topicName))
            {
                namespaceManager.CreateTopic(new TopicDescription(topicName)
                {
                    EnablePartitioning = _config.UsePartitioning
                });
            }

            var client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, _config.SubscriberName);
            RuleBuilder ruleBuilder = new RuleBuilder(new RuleApplier(_log, client), new RuleVersionResolver(), _config.SubscriberName);

            var newRule = ruleBuilder.GenerateSubscriptionRule(messagesTypes);

            if (!namespaceManager.SubscriptionExists(topicName, _config.SubscriberName))
            {
                namespaceManager.CreateSubscription(subscriptionDescription, newRule);
            }
            else
            {
                var existingRules = namespaceManager.GetRules(topicName, _config.SubscriberName).ToArray();
                ruleBuilder.BuildRules(newRule, existingRules, messagesTypes);
            }
        
        }
    }
}
