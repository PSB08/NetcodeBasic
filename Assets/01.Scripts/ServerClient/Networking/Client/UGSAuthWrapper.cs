using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;


public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}



public class UGSAuthWrapper
{
    public static AuthState State { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (State == AuthState.Authenticated)
        {
            return State;
        }
        if (State == AuthState.Authenticating) //�̹� �����õ����̶��
        {
            Debug.LogWarning("Already authenticating!");
            await Authenticating(); //���
            return State;
        }

        await SignInAnonymouslyAsync(maxTries);
        return State;
    }

    private static async Task<AuthState> Authenticating()
    {
        while (State == AuthState.Authenticating || State == AuthState.NotAuthenticated)
        {
            await Task.Delay(500); //0.5�� ���
        }

        return State;
    }

    private static async Task SignInAnonymouslyAsync(int maxTries)
    {
        State = AuthState.Authenticating;

        int tries = 0;

        while (State == AuthState.Authenticating && tries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    State = AuthState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError(ex);
                State = AuthState.Error;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError(ex);
                State = AuthState.Error;
            }

            tries++;
            await Task.Delay(2000); //2�ʿ� �ѹ��� ���� �õ�
        }

        if (State != AuthState.Authenticated)
        {
            Debug.LogWarning($"UGS not signed in successfully after : {tries} tries");
            State = AuthState.TimeOut;
        }

    }

}
