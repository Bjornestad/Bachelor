using System;
using System.Collections.Generic;
using Bachelor.Services;
using Avalonia.Input;
using Bachelor.ViewModels;
using InputSimulatorStandard.Native;
using Moq;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest;

public class InputServiceTest
{
    private readonly InputService _inputService;


    public InputServiceTest()
    {
        var mockOutputViewModel = new Mock<OutputViewModel>();
        _inputService = new InputService(mockOutputViewModel.Object);
    }

    [Fact]
    public void MapAvaloniaKeyToMacKeyCode_ReturnsCorrectMacKeyCode()
    {
        var key = Key.A;

        var macKeyCode = _inputService.MapAvaloniaKeyToMacKeyCode(key);

        Assert.Equal(0, macKeyCode); 
    }

    [Fact]
    public void MapAvaloniaKeyToMacKeyCode_ReturnsDefaultForUnmappedKey()
    {
        var key = Key.None;

        var macKeyCode = _inputService.MapAvaloniaKeyToMacKeyCode(key);

        Assert.Equal(49, macKeyCode); 
    }

    [Fact]
    public void MapAvaloniaKeyToVirtualKey_ReturnsCorrectVirtualKeyCode()
    {
        var key = Key.A;

        var virtualKeyCode = _inputService.MapAvaloniaKeyToVirtualKey(key);

        Assert.Equal(VirtualKeyCode.VK_A, virtualKeyCode);
    }

    [Fact]
    public void MapAvaloniaKeyToVirtualKey_ReturnsDefaultForUnmappedKey()
    {
        var key = Key.None;

        var virtualKeyCode = _inputService.MapAvaloniaKeyToVirtualKey(key);

        Assert.Equal(VirtualKeyCode.SPACE, virtualKeyCode); 
    }

    [Fact]
    public void MapAvaloniaKeyToVirtualKey_HandlesDynamicMapping()
    {
        var key = Key.Z;

        var virtualKeyCode = _inputService.MapAvaloniaKeyToVirtualKey(key);

        Assert.Equal(VirtualKeyCode.VK_Z, virtualKeyCode);
    }
}