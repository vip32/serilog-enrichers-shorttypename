using NSubstitute;
using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System.Collections.Generic;

namespace Serilog.Enrichers.ShortTypeName.Tests
{
    public class Tests
    {
        [Test]
        [TestCase("Assembly.MyType")]
        [TestCase("Assembly.Very.Long.Namespace.MyType")]
        [TestCase("MyType")]
        public void Enrich_ShouldShortenName(string longName)
        {
            // Arrange
            var logProperties = new List<LogEventProperty>()
            {
                new LogEventProperty("SourceContext", new ScalarValue(longName))
            };
            var logEvent = new LogEvent(new System.DateTimeOffset(), LogEventLevel.Information, null, new MessageTemplate(new List<MessageTemplateToken>()), logProperties);

            var subject = new ShortTypeNameEnricher();

            var propertyFactory = Substitute.For<ILogEventPropertyFactory>();
            propertyFactory.CreateProperty(Arg.Is("ShortTypeName"), Arg.Is("MyType")).Returns(new LogEventProperty("ShortTypeName", new ScalarValue("MyType")));

            // Act
            subject.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.AreEqual("MyType", ((ScalarValue)logEvent.Properties["ShortTypeName"]).Value);
        }

        [Test]
        public void Enrich_ShouldHandleNoSourceContext()
        {
            // Arrange
            var logProperties = new List<LogEventProperty>();
            var logEvent = new LogEvent(new System.DateTimeOffset(), LogEventLevel.Information, null, new MessageTemplate(new List<MessageTemplateToken>()), logProperties);

            var subject = new ShortTypeNameEnricher();

            var propertyFactory = Substitute.For<ILogEventPropertyFactory>();

            // Act
            subject.Enrich(logEvent, propertyFactory);

            // Assert
            propertyFactory.Received(0).CreateProperty(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        [TestCase("Assembly.")]
        [TestCase(".MyType")]
        [TestCase("...")]
        public void Enrich_ShouldHandleBrokenNameWithoutCrashing(string longName)
        {
            // Arrange
            var logProperties = new List<LogEventProperty>()
            {
                new LogEventProperty("SourceContext", new ScalarValue(longName))
            };
            var logEvent = new LogEvent(new System.DateTimeOffset(), LogEventLevel.Information, null, new MessageTemplate(new List<MessageTemplateToken>()), logProperties);

            var subject = new ShortTypeNameEnricher();

            var propertyFactory = Substitute.For<ILogEventPropertyFactory>();
            propertyFactory.CreateProperty(Arg.Is("ShortTypeName"), Arg.Any<string>()).Returns(new LogEventProperty("ShortTypeName", new ScalarValue("MyType")));

            // Act
            subject.Enrich(logEvent, propertyFactory);

            // Assert
            propertyFactory.Received(1).CreateProperty(Arg.Any<string>(), Arg.Any<object>());
        }
    }
}