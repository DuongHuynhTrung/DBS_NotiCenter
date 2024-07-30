using AgoraIO.Media;
using Data.Models;

public interface IAgoraService
{
    Task<ResultModel> GetRtcToken(CallTokenModel model);
}
public class AgoraService : IAgoraService
{
    private const string _appId = "19ecab3347004e29a4c02857f983eab4";
    private const string _appCertificate = "6e14c24081484f60abd19a9fead98af3";
    private uint _expireTimeInSeconds = 3600;
    private uint _salt = 1;
    private string _uid = "0";

    public async Task<ResultModel> GetRtcToken(CallTokenModel model)
    {
        var result = new ResultModel();
        result.Succeed = true;
        try
        {
            uint privilegeExpiredTs = _expireTimeInSeconds + (uint)Utils.getTimestamp();
            AccessToken accessToken = new AccessToken(_appId, _appCertificate, model.ChannelName, _uid, privilegeExpiredTs, _salt);
            accessToken.AddPrivilege(Privileges.kJoinChannel, privilegeExpiredTs);
            accessToken.AddPrivilege(Privileges.kPublishAudioStream, privilegeExpiredTs);
            accessToken.AddPrivilege(Privileges.kPublishVideoStream, privilegeExpiredTs);
            accessToken.AddPrivilege(Privileges.kPublishDataStream, privilegeExpiredTs);
            string token = accessToken.Build();
            result.Data = token;
        }
        catch (Exception e)
        {
            result.Succeed = false;
            result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
        }
        return result;
    }
}