public class PostData
{
    public string user_auth;
    public string user_pass_auth;
    public int server_id;
}

public class PlayerCounterPostData : PostData
{
    public int players_count;
}

public class StatusPostData : PostData
{
    public string status;
}