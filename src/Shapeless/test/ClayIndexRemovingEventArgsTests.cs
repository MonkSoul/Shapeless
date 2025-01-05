﻿// 版权归百小僧及百签科技（广东）有限公司所有。
// 
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace Shapeless.Tests;

public class ClayIndexRemovingEventArgsTests
{
    [Fact]
    public void New_ReturnOK()
    {
        var eventArgs = new ClayIndexRemovingEventArgs("Name");
        Assert.Equal("Name", eventArgs.KeyOrIndex);

        var eventArgs2 = new ClayIndexRemovingEventArgs(0);
        Assert.Equal(0, eventArgs2.KeyOrIndex);
    }
}