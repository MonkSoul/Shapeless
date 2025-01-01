﻿// 版权归百小僧及百签科技（广东）有限公司所有。
// 
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace Shapeless.Tests;

public class Utf8StringWriterTests
{
    [Fact]
    public void New_ReturnOK()
    {
        using var writer = new Utf8StringWriter();
        Assert.Equal(Encoding.UTF8, writer.Encoding);
    }
}