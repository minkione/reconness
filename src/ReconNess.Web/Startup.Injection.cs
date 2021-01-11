using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReconNess.Core;
using ReconNess.Core.Providers;
using ReconNess.Core.Services;
using ReconNess.Data.Npgsql;
using ReconNess.Providers;
using ReconNess.PubSub;
using ReconNess.Services;
using ReconNess.Web.Auth;
using ReconNess.Worker;

namespace ReconNess.Web
{
    public partial class Startup
    {
        private void AddDependencyInjection(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddSingleton<IJwtFactory, JwtFactory>();

            services.AddScoped<IDbContext, ReconNessContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();            

            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IAgentRunnerService, AgentRunnerService>();
            services.AddScoped<IAgentCategoryService, AgentCategoryService>();
            services.AddScoped<ITargetService, TargetService>();
            services.AddScoped<IRootDomainService, RootDomainService>();
            services.AddScoped<ISubdomainService, SubdomainService>();
            services.AddScoped<INotesService, NotesService>();
            services.AddScoped<ILabelService, LabelService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IReferenceService, ReferenceService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuthProvider, AuthProvider>();

            services.AddSingleton<IAgentRunService, AgentRunService>();
            services.AddSingleton<IAgentBackgroundService, AgentBackgroundService>();
            services.AddSingleton<IConnectorService, ConnectorService>();
            services.AddSingleton<IVersionProvider, VersionProvider>();
            services.AddSingleton<ILogsProvider, LogsProvider>();

            services.AddSingleton<IScriptEngineService, ScriptEngineService>();
            services.AddSingleton<IAgentRunnerProvider, QueueAgentRunnerProvider>();
            //services.AddSingleton<IAgentRunnerProvider, WorkerAgentRunnerProvider>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }
    }
}
