////@implements IDisposable
//namespace QMM.Components {
//    public class Timer {
//        private CancellationTokenSource source = new CancellationTokenSource();
//        private CancellationToken token;

//        PeriodicTimer periodicTimer = new();

//        public void Start() {
//            token = source.Token;
//            RunTimer();  // fire-and-forget
//        }

//        public void Stop() {
//            source.Cancel();
//        }

//        async void RunTimer() {
//            while (await periodicTimer.WaitForNextTickAsync(token)) {

//            }
//        }

//        public void Dispose() {
//            periodicTimer?.Dispose();
//        }
//    }
//}
