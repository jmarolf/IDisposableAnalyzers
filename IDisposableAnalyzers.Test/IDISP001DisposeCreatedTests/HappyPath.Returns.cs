namespace IDisposableAnalyzers.Test.IDISP001DisposeCreatedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        public class Returns
        {
            [Test]
            public void SimpleStatementBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar()
        {
            return File.OpenRead(string.Empty);
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void SimpleExpressionBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar() => File.OpenRead(string.Empty);
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void LocalFileOpenRead()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar()
        {
            var stream = File.OpenRead(string.Empty);
            return stream;
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void LocalFileOpenReadAfterAccessingLength()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar()
        {
            var stream = File.OpenRead(string.Empty);
            var length = stream.Length;
            return stream;
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void LocalInIfAndEnd()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar()
        {
            var stream = File.OpenRead(string.Empty);
            if (true)
            {
                return stream;
            }

            return stream;
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void LocalInIf()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar(string text)
        {
            var stream = File.OpenRead(string.Empty);
            if (text == null)
            {
                return stream;
            }

            var length = stream.Length;
            return stream;
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void LocalInStreamReaderMethodBody()
            {
                var testCode = @"
    using System.IO;

    public static class Foo
    {
        public static StreamReader Bar()
        {
            var stream = File.OpenRead(string.Empty);
            return new StreamReader(stream);
        }
    }";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void FileOpenReadIsReturnedInCompositeDisposableMethodBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;
    using System.Reactive.Disposables;

    public static class Foo
    {
        public static CompositeDisposable Bar()
        {
            var stream = File.OpenRead(string.Empty);
            return new CompositeDisposable { stream };
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void WhenDisposableIsReturnedPropertySimple()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar
        {
            get
            {
                return File.OpenRead(string.Empty);;
            }
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void WhenDisposableIsReturnedPropertyBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar
        {
            get
            {
                var stream = File.OpenRead(string.Empty);
                return stream;
            }
        }
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }

            [Test]
            public void WhenDisposableIsReturnedPropertyExpressionBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public static class Foo
    {
        public static Stream Bar => File.OpenRead(string.Empty);
    }
}";
                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }
        }
    }
}