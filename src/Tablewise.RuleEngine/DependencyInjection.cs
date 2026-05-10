using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tablewise.RuleEngine.Interfaces;
using Tablewise.RuleEngine.Services;

namespace Tablewise.RuleEngine;

/// <summary>
/// Rule Engine DI extension metodları.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Rule Engine servislerini DI container'a ekler.
    /// Tablewise.Api'den çağrılmalı.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection (fluent)</returns>
    public static IServiceCollection AddRuleEngine(this IServiceCollection services)
    {
        // Tüm IRuleTypeEvaluator implementasyonlarını tara ve register et
        var assembly = Assembly.GetExecutingAssembly();
        var evaluatorTypes = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(IRuleTypeEvaluator).IsAssignableFrom(t));

        foreach (var type in evaluatorTypes)
        {
            services.AddScoped(typeof(IRuleTypeEvaluator), type);
        }

        // Factory ve Pipeline
        services.AddScoped<IRuleTypeEvaluatorFactory, RuleTypeEvaluatorFactory>();
        services.AddScoped<IRuleEnginePipeline, RuleEnginePipeline>();

        return services;
    }

    /// <summary>
    /// Rule Engine servislerini belirtilen assembly'den ekler.
    /// Custom evaluator'lar için kullanılır.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Taranacak assembly'ler</param>
    /// <returns>Service collection (fluent)</returns>
    public static IServiceCollection AddRuleEngine(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Temel servisleri ekle
        services.AddRuleEngine();

        // Ek assembly'leri tara
        foreach (var assembly in assemblies)
        {
            var evaluatorTypes = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(IRuleTypeEvaluator).IsAssignableFrom(t));

            foreach (var type in evaluatorTypes)
            {
                services.AddScoped(typeof(IRuleTypeEvaluator), type);
            }
        }

        return services;
    }
}
