namespace Models {
    public class AwsComponent{
        private string arn;
        private string instanceName;
        private InstanceType iType;

        public string Arn => arn;
        public string InstanceName => instanceName;
        public InstanceType IType => iType;

        public AwsComponent(string arn, string instanceName, InstanceType iType) {
            this.arn = arn;
            this.instanceName = instanceName;
            this.iType = iType;
        }
    }

    public enum InstanceType{
        EC2,
        RDB,
        ALB
    }
}