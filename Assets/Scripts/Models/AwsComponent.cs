using System.Security.Cryptography.X509Certificates;

namespace Models {
    public class AwsComponent{
        private string arn;
        private string instanceName;
        private InstanceType iType;
        private InstanceState iState;
        private float cost;

        public string Arn => arn;
        public string InstanceName => instanceName;
        public InstanceType IType => iType;
        public InstanceState IState => iState;
        public float Cost => cost;

        public AwsComponent(string arn, string instanceName, InstanceType iType, InstanceState iState, float cost = 0f) {
            this.arn = arn;
            this.instanceName = instanceName;
            this.iType = iType;
            this.iState = iState;
            this.cost = cost;
        }
    }

    public enum InstanceType{
        EC2,
        RDS,
        ALB
    }
    public enum InstanceState {
        HIGH,
        MIDDLE,
        LOW
    }

    public static class InstanceStateHelper {

        public static float InstanceStateSpeeder(InstanceState state) {
            switch (state) {
                case InstanceState.HIGH:
                    return 0.5f;
                case InstanceState.MIDDLE:
                    return 1.0f;
                case InstanceState.LOW:
                    return 1.3f;
            }

            return 1.0f;
        }
    }
}