namespace FilesService.Models
{
    public class MinIOConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool Secure { get; set; } = false;
    }
}