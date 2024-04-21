using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace IP.Project.Tests.Base
{
    internal class AsyncHelper
    {
        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _innerQueryProvider;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _innerQueryProvider = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression) => _innerQueryProvider.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _innerQueryProvider.Execute<TResult>(expression);

            TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                Type expectedResultType = typeof(TResult).GetGenericArguments()[0];
                object? executionResult = ((IQueryProvider)this).Execute(expression);

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _enumerator = inner;
            }

            public T Current => _enumerator.Current;

            public ValueTask DisposeAsync() => new(Task.Run(() => _enumerator.Dispose()));

            public ValueTask<bool> MoveNextAsync() => new(_enumerator.MoveNext());
        }
    }
}