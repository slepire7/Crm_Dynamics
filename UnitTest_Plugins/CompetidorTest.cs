using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using System.Linq;
using System.Security.Principal;

namespace UnitTest_Plugins
{
    public class CompetidorTest : Config.BaseUnitTest
    {
        const string TableCompetitorName = "competitor";
        public CompetidorTest() : base(true) { }

        public override void SetConfigPluginSTeps()
        {
            _context.RegisterPluginStep<Plugins.OnCreateCompetitor_Validation>(new PluginStepDefinition()
            {
                EntityLogicalName = TableCompetitorName,
                MessageName = "Create",
                Stage = FakeXrmEasy.Abstractions.Plugins.Enums.ProcessingStepStage.Prevalidation,
                FilteringAttributes = new string[2]
                           {
                                    "name","websiteurl"
                           },
            });
        }
        [Test]
        public void Test_Validation_WebSite()
        {

            var inputParams = new ParameterCollection();
            inputParams.Add("Target", new Entity(TableCompetitorName)
            {
                ["name"] = "Xpto10",
                ["websiteurl"] = "https://www.google.com"
            });
            try
            {
                var result = _context.ExecutePluginWith<Plugins.OnCreateCompetitor_Validation>(inputParams, outputParameters: null, preEntityImages: null, postEntityImages: null);
                Assert.Fail();
            }
            catch (InvalidPluginExecutionException e)
            {
                Assert.Pass();
            }

        }

        [Test]
        [Description("Teste criado para rodar todos os plugins no evento de create para a tabela Competidor")]
        public void Test_End_To_End_Success()
        {
            SetConfigPluginSTeps();
            var competidor_new = new Entity(TableCompetitorName);
            competidor_new["name"] = "Xpt01";
            competidor_new["websiteurl"] = "https://www.jreis.com";
            _service.Create(competidor_new);
            var result = _context.CreateQuery(TableCompetitorName).FirstOrDefault(competitor => competitor.GetAttributeValue<string>("name") == "Xpt01");

            Assert.IsTrue(result != null);
        }
    }
}