using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ReactiveDomain.Messaging;

namespace ReactiveDomain.Bus
{
    public class CommandTracker : IDisposable

    {
        private static readonly Logger Log = NLog.LogManager.GetLogger("ReactiveDomain");
        private readonly Command _command;
        private readonly TaskCompletionSource<CommandResponse> _tcs;
        private readonly TimeSpan _responseTimeout;
        private readonly CancellationTokenSource _canelTokenSource;
        private readonly Action _completionAction;
        private readonly Action _cancelAction;
        private bool _disposed = false;

        private const long PendingAck = 0;
        private const long PendingResponse = 1;
        private const long Complete = 2;
        private long _state;

        public CommandTracker(
            Command command,
            TaskCompletionSource<CommandResponse> tcs,
            Action completionAction,
            Action cancelAction,
            TimeSpan ackTimeout,
            TimeSpan responseTimeout)
        {
            _canelTokenSource = new CancellationTokenSource();
            _command = command;
            _tcs = tcs;
            _responseTimeout = responseTimeout;
            _completionAction = completionAction;
            _cancelAction = cancelAction;
            _state = PendingAck;

            Task.Delay(ackTimeout, _canelTokenSource.Token).ContinueWith(_ => AckTimeout());

        }

        public void Handle(CommandResponse message)
        {
            Interlocked.Exchange(ref _state, Complete);
            if (_tcs.TrySetResult(message)) _completionAction();
        }

        private long _ackCount = 0;
        public void Handle(AckCommand message)
        {
            Interlocked.Increment(ref _ackCount);
            var curState = Interlocked.Read(ref _state);
            if (curState != PendingAck || Interlocked.CompareExchange(ref _state, PendingResponse, curState) != curState)
            {
                if (Log.IsErrorEnabled)
                    Log.Error(_command.GetType().Name + " Multiple Handlers Acked Command");
                if (_tcs.TrySetException(new CommandOversubscribedException(" multiple handlers responded to the command", _command)))
                    _cancelAction();
                return;
            }
            Task.Delay(_responseTimeout, _canelTokenSource.Token).ContinueWith(_ => ResponseTimeout());
        }

        public void AckTimeout()
        {
            if (Interlocked.Read(ref _state) == PendingAck)
            {
                if (_tcs.TrySetException(new CommandNotHandledException(" timed out waiting for handler to start.", _command)))
                {
                    if (Log.IsErrorEnabled)
                        Log.Error(_command.GetType().Name + " command not handled (no handler)");
                    _cancelAction();
                }
            }
        }

        public void ResponseTimeout()
        {
            if (Interlocked.Read(ref _state) == PendingResponse)
            {
                if (_tcs.TrySetException(new CommandTimedOutException(" timed out waiting for handler to complete.", _command)))
                {
                    if (Log.IsErrorEnabled)
                        Log.Error(_command.GetType().Name + " command timed out");
                    _cancelAction();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (!_tcs.Task.IsCanceled && !_tcs.Task.IsCompleted && !_tcs.Task.IsFaulted)
                {
                    _tcs.TrySetCanceled();
                }
                _tcs.Task.Dispose();
                _canelTokenSource.Dispose();

            }
            _disposed = true;
        }
    }
}