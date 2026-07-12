using Robust.Serialization;

namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {


        public override void Initialize() {
            base.Initialize();

            SubscribleLocalEvent<PdaComponent, PdaNotificationEvent>(OnPdaNotification)
        }

        public void OnPdaNotification()
        {
            var query = EntityQueryEnumerator<PdaComponent>;

            while (query.MoveNext(out var uid, out _)) {

            }
        }
    }
}
