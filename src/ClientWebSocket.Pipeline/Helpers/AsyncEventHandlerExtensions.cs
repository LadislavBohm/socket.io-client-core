using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientWebSocket.Pipeline.Helpers
{
    public static class AsyncEventHandlerExtensions
    {
                /// <summary>
        /// Invokes asynchronous event handlers, returning a task that completes when all event handlers have been invoked.
        /// Each handler is fully executed (including continuations) before the next handler in the list is invoked.
        /// </summary>
        /// <param name="handlers">The event handlers.  May be <c>null</c></param>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event argument.</param>
        /// <returns>The task that completes when all handlers have completed.</returns>
        /// <exception cref="AggregateException">Thrown if any handlers fail. It contains a collection of all failures.</exception>
        public static async ValueTask InvokeAsync(this AsyncEventHandler handlers, object sender, EventArgs args)
        {
            if (handlers != null)
            {
                var individualHandlers = handlers.GetInvocationList();
                List<Exception> exceptions = null;
                foreach (AsyncEventHandler handler in individualHandlers)
                {
                    try
                    {
                        await handler(sender, args);
                    }
                    catch (Exception ex)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>(2);
                        }

                        exceptions.Add(ex);
                    }
                }

                if (exceptions != null)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        /// <summary>
        /// Invokes asynchronous event handlers, returning a task that completes when all event handlers have been invoked.
        /// Each handler is fully executed (including continuations) before the next handler in the list is invoked.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of argument passed to each handler.</typeparam>
        /// <param name="handlers">The event handlers.  May be <c>null</c></param>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event argument.</param>
        /// <returns>The task that completes when all handlers have completed.  The task is faulted if any handlers throw an exception.</returns>
        /// <exception cref="AggregateException">Thrown if any handlers fail. It contains a collection of all failures.</exception>
        public static async ValueTask InvokeAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handlers, object sender, TEventArgs args)
        {
            if (handlers != null)
            {
                var individualHandlers = handlers.GetInvocationList();
                List<Exception> exceptions = null;
                foreach (AsyncEventHandler<TEventArgs> handler in individualHandlers)
                {
                    try
                    {
                        await handler(sender, args);
                    }
                    catch (Exception ex)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>(2);
                        }

                        exceptions.Add(ex);
                    }
                }

                if (exceptions != null)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }
    }
}
