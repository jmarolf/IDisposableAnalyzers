﻿namespace IDisposableAnalyzers.Test.IDISP006ImplementIDisposableTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        public class InterfaceOnlyMakeSealed
        {
            [Test]
            public void EmptyClass()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", testCode, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void EmptyClassFigureOutUnderscoreFromOtherClass()
            {
                var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        private int _value;

        public Bar(int value)
        {
            _value = value;
        }
    }
}";

                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", new[] { barCode, testCode }, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void WithThrowIfDisposed()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
        private void ThrowIfDisposed()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", testCode, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void WithProtectedPrivateSetProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
        protected int Value { get; private set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool disposed;

        private int Value { get; set; }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", testCode, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void WithOverridingProperties()
            {
                var baseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase
    {
        public virtual int Value1 { get; protected set; }

        public abstract int Value2 { get; set; }

        protected virtual int Value3 { get; set; }
    }
}";

                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : FooBase, ↓IDisposable
    {
        public override int Value1 { get; protected set; }

        public override int Value2 { get; set; }

        protected override int Value3 { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : FooBase, IDisposable
    {
        private bool disposed;

        public override int Value1 { get; protected set; }

        public override int Value2 { get; set; }

        protected override int Value3 { get; set; }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", new[] { baseCode, testCode }, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void WithPublicVirtualMethod()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
        public virtual void Bar()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool disposed;

        public void Bar()
        {
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", testCode, fixedCode, "Implement IDisposable and make class sealed.");
            }

            [Test]
            public void WithProtectedVirtualMethod()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : ↓IDisposable
    {
        protected virtual void Bar()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void Bar()
        {
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementIDisposableCodeFixProvider>("CS0535", testCode, fixedCode, "Implement IDisposable and make class sealed.");
            }
        }
    }
}