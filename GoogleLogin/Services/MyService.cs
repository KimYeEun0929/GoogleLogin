namespace GoogleLogin.Services
{
    public class MyService
    {
        private readonly HttpClient _httpClient;

        public MyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Kakao OAuth API에 인증 코드를 포함한 POST 요청을 보내고, 응답을 문자열로 반환합니다.
        public async Task<string> SendPostRequestAsync(string code)
        {
            const string grant_type = "authorization_code";
            const string client_id = ""; // 민감한 정보라 이걸 넣고서는 git push 안 됨.
            const string client_secret = ""; // 민감한 정보라 이걸 넣고서는 git push 안 됨.
            const string redirect_uri = "https://localhost:7185/oauth";

            string url = "https://oauth2.googleapis.com/token";

            // key-value 형식의 데이터
            var payload = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", grant_type),
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("client_secret", client_secret),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri),
                new KeyValuePair<string, string>("code", code)
            };

            // x-www-form-urlencoded 형식으로 Content 생성
            var content = new FormUrlEncodedContent(payload);

            // HTTP POST 요청 전송
            var response = await _httpClient.PostAsync(url, content);

            // 응답 확인
            response.EnsureSuccessStatusCode();

            //응답 본문 읽기
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SendGetRequestAsync(string access_token)
        {
            string url = "https://www.googleapis.com/userinfo/v2/me";

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

            // HTTP GET 요청 전송
            var response = await _httpClient.GetAsync(url);

            // 응답 확인
            response.EnsureSuccessStatusCode();

            // 응답 본문 읽기
            return await response.Content.ReadAsStringAsync();
        }

        //// 연결끊기 POST 요청
        public async Task<string> RevokeTokenAsync(string token)
        {
            string url = "https://oauth2.googleapis.com/revoke";

            // 요청 본문에 토큰을 전달
            var payload = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("token", token)
            };

            // x-www-form-urlencoded 형식의 Content 생성
            var content = new FormUrlEncodedContent(payload);

            // POST 요청 전송
            var response = await _httpClient.PostAsync(url, content);

            // 응답 상태 확인
            if (response.IsSuccessStatusCode)
            {
                return "Token revoked successfully.";
            }
            else
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to revoke token: {response.StatusCode}, Details: {errorDetails}");
            }
        }
    }
}
