﻿using FubuTestingSupport;
using NUnit.Framework;
using ripple.Model;
using ripple.Nuget;

namespace ripple.Testing.Nuget.Operations
{
    [TestFixture]
    public class update_an_existing_floated_dependency_with_new_transitive_dependencies : NugetOperationContext
    {
        private SolutionGraphScenario theScenario;
        private Solution theSolution;
        private NugetPlan thePlan;
        private NugetPlanBuilder theBuilder;

        [SetUp]
        public void SetUp()
        {
            FeedScenario.Create(scenario =>
            {
                scenario.For(Feed.Fubu)
                        .Add("Serenity", "1.1.0.0")
                        .Add("Serenity", "1.2.0.0")
                        .ConfigureRepository(fubu =>
                        {
                            fubu.ConfigurePackage("Serenity", "1.1.0.0", serenity => serenity.DependsOn("WebDriver", "1.1.0.0"));
                            fubu.ConfigurePackage("Serenity", "1.2.0.0", serenity =>
                            {
                                serenity.DependsOn("WebDriver", "1.2.0.0");
                                serenity.DependsOn("Something");
                                serenity.DependsOn("SomethingElse", "0.9.9.9");
                            });
                        });

                scenario.For(Feed.NuGetV2)
                        .Add("Something", "1.0.0.5")
                        .Add("SomethingElse", "0.9.9.9")
                        .Add("WebDriver", "1.1.0.0")
                        .Add("WebDriver", "1.2.0.0");
            });

            theScenario = SolutionGraphScenario.Create(scenario =>
            {
                scenario.Solution("Test", sln =>
                {
                    sln.SolutionDependency("WebDriver", "1.1.0.0", UpdateMode.Fixed);

                    sln.LocalDependency("Serenity", "1.1.0.0");

                    sln.ProjectDependency("Test1", "Serenity");
                    sln.ProjectDependency("Test2", "Serenity");
                });
            });

            theSolution = theScenario.Find("Test");

            theBuilder = new NugetPlanBuilder();

            var request = new NugetPlanRequest
            {
                Solution = theSolution,
                Dependency = new Dependency("Serenity"),
                Operation = OperationType.Update,
                ForceUpdates = false
            };

            thePlan = theBuilder.PlanFor(request);
        }

        [TearDown]
        public void TearDown()
        {
            theScenario.Cleanup();
            FeedRegistry.Reset();
        }

        [Test]
        public void verify_plan()
        {
            thePlan.ShouldHaveTheSameElementsAs(
                
                updateSolutionDependency("Serenity", "1.2.0.0", UpdateMode.Float),
                updateSolutionDependency("WebDriver", "1.2.0.0", UpdateMode.Fixed),
                
                solutionInstallation("Something", "1.0.0.5", UpdateMode.Fixed),
                solutionInstallation("SomethingElse", "0.9.9.9", UpdateMode.Fixed),

                projectInstallation("Test1", "Something"),
                projectInstallation("Test1", "SomethingElse"),

                projectInstallation("Test2", "Something"),
                projectInstallation("Test2", "SomethingElse")
            );
        }

    }
}