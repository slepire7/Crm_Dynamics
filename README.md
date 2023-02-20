# Crm_Dynamics
Olá! projeto desenvolvido com o objetivo didático.

# Features

* Implementação de Plugin implementando os conceitos de (DRY e Clean Code)
	* 	Apenas para a tabela Competidor **(Compentitor)** nos eventos de **Create** e nos Steps de **Pre Validation** e **Post Operation**
* Testes de Unidade (FakeXrmEasy) Isolados e tambem de Ponta-a-Ponta
	* podendo executar apenas um plugin ou vários

# Code + Explanation

## Plugins
utilizando o conceito DRY para iniciar a implementação do Plugin teriamos que repetir as abaixo para cada novo plugin que fosse ser implementado 

    public class Account_MarkDonw : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService _Logger = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService OrganizationService = serviceFactory.CreateOrganizationService(Context.InitiatingUserId);
        }
    }
para evitar isso foi implementado uma classe abstrata que vai centralizar essa implementação da interface IPlugin, juntamente com a classe PluginContext que ira ficar com o valor de determinadas propriedades do IServiceProvider.

    public abstract class Plugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginExecute(new PluginContext(serviceProvider));
        }
        public abstract void PluginExecute(PluginContext pluginContext);
    }
    public class PluginContext
    {
        ITracingService _Logger;
        public IPluginExecutionContext Context;
        public IOrganizationService OrganizationService;
        public PluginContext(IServiceProvider serviceProvider)
        {
            _Logger = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            OrganizationService = serviceFactory.CreateOrganizationService(Context.InitiatingUserId);
        }

        public void Trace(string message)
        {
            this._Logger.Trace(message);
        }

        public Entity GetTarget()
        {
            const string Key = "Target";
            return Context.InputParameters.ContainsKey(Key) ? (Entity)Context.InputParameters[Key] : default;
        }
        public TEntity GetTarget<TEntity>() where TEntity : BaseEntity
        {
            const string Key = "Target";
            return Context.InputParameters.ContainsKey(Key) ? ((Entity)Context.InputParameters[Key]).ToEntity<TEntity>() : default;
        }
    }


para utlizar é necessario fazer a referencia do projeto compartilhado Plugin.Shared em sua biblioteca de classes por exemplo 

      public class OnCreateCompetitor_Validation : Plugin.Shared.Base.Plugin
    {
        public const int DEPTH = 2;
        public override void PluginExecute(PluginContext pluginContext)
        {
            try
            {
                if (pluginContext.Context.Depth > DEPTH) return;
                var Competitor = pluginContext.GetTarget<Competitor>();
                Validation_WebSite(ref Competitor);

            }
            catch (InvalidPluginExecutionException ex) when (ex.InnerException != null)
            {
                pluginContext.Trace(ex.InnerException.Message);
                throw ex;
            }
            catch (InvalidPluginExecutionException ex)
            {
                pluginContext.Trace(ex.Message);
                throw ex;
            }
        }
        public void Validation_WebSite(ref Competitor Competitor)
        {
            string Url_Competitor = Competitor.WebSiteUrl;
            if (string.IsNullOrEmpty(Url_Competitor) == false)
            {
                Uri[] BLACK_LIST_WEBSITES = new Uri[2] {
                     new Uri("https://www.google.com")
                    ,new Uri("https://www.bing.com")
                };

                if (BLACK_LIST_WEBSITES.Any(uri => uri.Host == new Uri(Url_Competitor).Host))
                    throw new InvalidPluginExecutionException($"o site: {Url_Competitor} não pode ser utilizado", PluginHttpStatusCode.BadRequest);
            }
        }
    }

## Unit Test
para os testes unitários  esta sendo utilizada a feature **PipelineSimulation** com ela é possivel simular os efeitos em cascata Exemplo: 
caso tenha mais de um plugin para determinada **Message**,**Fields** e **Steps** eles serão chamados na sequencia de acordo com a configuração que sera feita no metodo abstract **SetConfigPluginSTeps** de BaseUnitTest.cs que sera herdado no seu teste,
Alem disso é possivel se autenticar a qualquer ambiente que você tenhas as credencias precisa somente passar no construtor herdado o valor de "false" que vai ser chamado o metodo **GetOrganization_Env** em seguida vai buscar a variavel **Dynamics_ConnectionString** necessario preencher a url do ambiente, usuario e senha.
     

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
a utilização dele é bem simples.

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
            _context.RegisterPluginStep<Plugins.OnCreateCompetitor_PreOperation>(new PluginStepDefinition()
            {
                EntityLogicalName = TableCompetitorName,
                MessageName = "Create",
                Stage = FakeXrmEasy.Abstractions.Plugins.Enums.ProcessingStepStage.Postoperation,
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
            inputParams.Add("Target", new Competitor()
            {
                Name = "Xpto10",
                WebSiteUrl = "https://www.google.com"
            });
            try
            {
                var result = _context.ExecutePluginWith<Plugins.OnCreateCompetitor_Validation>(inputParams, outputParameters: null, preEntityImages: null, postEntityImages: null);
                Assert.Fail();
            }
            catch (InvalidPluginExecutionException e)
            {
                Assert.Pass(e.Message);
            }

        }

        [Test]
        [Description("Teste criado para rodar todos os plugins no evento de create para a tabela Competidor")]
        public void Test_End_To_End_Success()
        {
            SetConfigPluginSTeps();
            var competidor_new = new Competitor();
            competidor_new.Name = "Xpt01";
            competidor_new.WebSiteUrl = "https://www.jreis.com";
            _service.Create(competidor_new);
            var result = _context.CreateQuery<Competitor>().FirstOrDefault(competitor => competitor.Name == competidor_new.Name);

            Assert.IsTrue(result != null);
        }
    }
com isso podemos mitigar alguns dos problemas que podemos ter com o plugin no ambiente de DEV antes mesmo de publicar como:
* Loop Infinito Exemplo: 
			No evento de Create do plugin tem um metodo que é para realizar a copia desse registro iniciando mais um evento de Create e assim gerando um loop infinito.
* Validar de cenarios pre-definidos.
* Controle de código.
* Paz de espirito 🍷🗿
# Publication

Use o Plugin Registration [Tutorial: Write and register a plug-in (Microsoft Dataverse) - Power Apps | Microsoft Learn](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/tutorial-write-plug-in)
