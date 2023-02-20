using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using NUnit.Framework;
using System;
using System.Reflection;
using UnitTest_Plugins.Helpers;

namespace UnitTest_Plugins.Config
{
    public abstract class BaseUnitTest
    {
        protected readonly IOrganizationService _service;
        protected readonly IXrmFakedContext _context;

        public BaseUnitTest(bool UseFakeIOrganizationService)
        {
            _context = MiddlewareBuilder
                .New()

                .AddCrud()
                .AddFakeMessageExecutors(Assembly.GetAssembly(typeof(AddListMembersListRequestExecutor)))
                .AddPipelineSimulation()


                .UsePipelineSimulation()
                .UseCrud()
                .UseMessages()
            .SetLicense(FakeXrmEasyLicense.RPL_1_5)
            .Build();

            _service = UseFakeIOrganizationService ? _context.GetOrganizationService() : GetOrganization_Env();
        }
        private IOrganizationService GetOrganization_Env()
        {
            string _connectionString = AppSettings.GetEnv("Dynamics_ConnectionString");
            using (var crmClient = new CrmServiceClient(_connectionString))
            {
                return (IOrganizationService)crmClient.OrganizationServiceProxy ?? crmClient.OrganizationWebProxyClient;
            }
        }
        public abstract void SetConfigPluginSTeps();
    }
}
