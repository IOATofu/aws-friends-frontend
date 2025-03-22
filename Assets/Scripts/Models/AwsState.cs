namespace Models {
    public class AwsState {
        private string arn;
        private string instanceName;
        private InstanceState iState;

        public string Arn => arn;
        public string InstanceName => instanceName;
        public InstanceState IState => iState;

        public AwsState(string arn, string instanceName, InstanceState iState) {
            this.arn = arn;
            this.instanceName = instanceName;
            this.iState = iState;
        }
    }


    public enum InstanceState {
        HIGH,
        MIDDLE,
        LOW
    }
}