﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Xunit;

namespace SemanticKernel.Plugins.UnitTests.MsGraph;

public class CalendarPluginTests
{
    [Fact]
    public async Task AddEventAsyncSucceedsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anySubject = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        CalendarEvent expected = new()
        {
            Subject = anySubject,
            Location = anyLocation,
            Attendees = anyAttendees
        };

        Mock<ICalendarConnector> connectorMock = new();
        connectorMock.Setup(c => c.AddEventAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        CalendarPlugin target = new(connectorMock.Object);

        // Act
        var context = await KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["start"] = anyStartTime,
            ["end"] = anyEndTime,
            ["location"] = anyLocation,
            ["content"] = anyContent,
            ["attendees"] = string.Join(";", anyAttendees)
        });

        // Assert
        connectorMock.VerifyAll();
    }

    [Fact]
    public async Task AddEventAsyncWithoutLocationSucceedsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anySubject = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        CalendarEvent expected = new()
        {
            Content = anyContent,
            Subject = anySubject,
            Attendees = anyAttendees,
            Start = anyStartTime,
            End = anyEndTime
        };

        Mock<ICalendarConnector> connectorMock = new();
        connectorMock.Setup(c => c.AddEventAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        CalendarPlugin target = new(connectorMock.Object);

        // Act
        var context = await KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["start"] = anyStartTime,
            ["end"] = anyEndTime,
            ["content"] = anyContent,
            ["attendees"] = string.Join(";", anyAttendees),
        });

        // Assert
        connectorMock.VerifyAll();
    }

    [Fact]
    public async Task AddEventAsyncWithoutContentSucceedsAsync()
    {
        // Arrange
        string anySubject = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        CalendarEvent expected = new()
        {
            Subject = anySubject,
            Start = anyStartTime,
            End = anyEndTime,
            Location = anyLocation,
            Attendees = anyAttendees
        };

        Mock<ICalendarConnector> connectorMock = new();
        connectorMock.Setup(c => c.AddEventAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        CalendarPlugin target = new(connectorMock.Object);

        // Act
        var context = await KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["start"] = anyStartTime,
            ["end"] = anyEndTime,
            ["location"] = anyLocation,
            ["attendees"] = string.Join(";", anyAttendees),
        });

        // Assert
        connectorMock.VerifyAll();
    }

    [Fact]
    public async Task AddEventAsyncWithoutAttendeesSucceedsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anySubject = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);

        CalendarEvent expected = new()
        {
            Subject = anySubject,
            Start = anyStartTime,
            End = anyEndTime,
            Content = anyContent,
            Location = anyLocation
        };

        Mock<ICalendarConnector> connectorMock = new();
        connectorMock.Setup(c => c.AddEventAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        CalendarPlugin target = new(connectorMock.Object);

        // Act
        var context = await KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["start"] = anyStartTime,
            ["end"] = anyEndTime,
            ["location"] = anyLocation,
            ["attendees"] = anyContent,
        });

        // Assert
        connectorMock.VerifyAll();
    }

    [Fact]
    public async Task AddEventAsyncWithoutStartFailsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anySubject = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        Mock<ICalendarConnector> connectorMock = new();

        CalendarPlugin target = new(connectorMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<KernelException>(() => KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["end"] = anyEndTime,
            ["location"] = anyLocation,
            ["content"] = anyContent,
            ["attendees"] = string.Join(";", anyAttendees),
        }));
    }

    [Fact]
    public async Task AddEventAsyncWithoutEndFailsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anySubject = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        Mock<ICalendarConnector> connectorMock = new();

        CalendarPlugin target = new(connectorMock.Object);

        // Act
        await Assert.ThrowsAsync<KernelException>(() => KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["input"] = anySubject,
            ["start"] = anyStartTime,
            ["location"] = anyLocation,
            ["content"] = anyContent,
            ["attendees"] = string.Join(";", anyAttendees),
        }));
    }

    [Fact]
    public async Task AddEventAsyncWithoutSubjectFailsAsync()
    {
        // Arrange
        string anyContent = Guid.NewGuid().ToString();
        string anyLocation = Guid.NewGuid().ToString();
        DateTimeOffset anyStartTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        DateTimeOffset anyEndTime = DateTimeOffset.Now + TimeSpan.FromDays(1.1);
        string[] anyAttendees = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

        Mock<ICalendarConnector> connectorMock = new();

        CalendarPlugin target = new(connectorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KernelException>(() => KernelPluginFactory.CreateFromObject(target)["AddEvent"].InvokeAsync(new(), new()
        {
            ["start"] = anyStartTime,
            ["end"] = anyEndTime,
            ["location"] = anyLocation,
            ["content"] = anyContent,
            ["attendees"] = string.Join(";", anyAttendees),
        }));

        Assert.True(ex.InnerException is ArgumentException);
        Assert.Equal("input", ((ArgumentException)ex.InnerException).ParamName);
    }
}
