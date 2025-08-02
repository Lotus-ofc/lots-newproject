using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }

    public event Action<string> OnAuthFailed;    // Erro de autenticação
    public event Action OnAuthSuccess;           // Login bem-sucedido
    public event Action OnLogout;                // Logout

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseManager: Instância criada.");
            InitializeFirebase();
        }
        else
        {
            Debug.LogWarning("FirebaseManager: Instância já existe. Destruindo esta nova.");
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("FirebaseManager: Checando dependências do Firebase...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Auth.StateChanged += AuthStateChanged;
                User = Auth.CurrentUser;
                Debug.Log("✅ FirebaseManager: Firebase inicializado com sucesso.");
                // Chame AuthStateChanged no thread principal para garantir que os eventos Unity sejam disparados
                UnityMainThreadDispatcher.Instance().Enqueue(() => AuthStateChanged(this, EventArgs.Empty)); 
            }
            else
            {
                Debug.LogError($"❌ FirebaseManager: Erro ao inicializar Firebase: {status}");
                // Dispare o evento OnAuthFailed no thread principal
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Falha ao inicializar autenticação."));
            }
        });
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseUser newUser = Auth.CurrentUser;

        if (newUser != User)
        {
            User = newUser;
            if (User != null && User.IsValid())
            {
                Debug.Log($"✅ FirebaseManager: Usuário logado: {User.Email}");
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthSuccess?.Invoke()); // Garante no thread principal
            }
            else
            {
                Debug.Log("⚠️ FirebaseManager: Usuário deslogado.");
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnLogout?.Invoke()); // Garante no thread principal
            }
        }
        else if (User != null && User.IsValid())
        {
            Debug.Log($"FirebaseManager: Usuário já estava logado: {User.Email}");
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthSuccess?.Invoke()); // Garante no thread principal
        }
        else
        {
            Debug.Log("FirebaseManager: Ninguém logado.");
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnLogout?.Invoke()); // Garante no thread principal
        }
    }

    private void OnDestroy()
    {
        if (Auth != null)
        {
            Auth.StateChanged -= AuthStateChanged;
        }
    }

    public void RegisterUser(string email, string password)
    {
        Debug.Log($"FirebaseManager: Tentando registrar usuário: {email}");
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Preencha email e senha para registro."));
            return;
        }
        if (Auth == null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Serviço de autenticação não inicializado."));
            return;
        }

        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    string erro = GetFirebaseError(task.Exception);
                    Debug.LogError($"❌ Erro ao registrar: {erro}");
                    OnAuthFailed?.Invoke(erro); // Evento para notificar a falha de registro
                }
                else
                {
                    Debug.Log($"✅ Usuário registrado: {task.Result.User.Email}");
                    // O Firebase automaticamente loga o usuário após o registro bem-sucedido.
                    // O AuthStateChanged vai capturar isso e disparar OnAuthSuccess.
                    // Para feedback específico de "registro bem-sucedido" antes do login,
                    // você pode adicionar um novo evento OnRegisterSuccess.
                    // Por ora, confiaremos no fluxo AuthStateChanged -> OnAuthSuccess
                    // ou no feedback manual que o SceneFlowManager pode exibir após o OnRegisterButtonClicked.
                    SceneFlowManager.Instance.ShowOnlyPanel(SceneFlowManager.Instance.loginPanel); // Redireciona para o login
                    SceneFlowManager.Instance?.ShowFeedback("Usuário registrado com sucesso! Faça login."); // Feedback manual
                }
            });
        });
    }

    public void LoginUser(string email, string password)
    {
        Debug.Log($"FirebaseManager: Tentando logar usuário: {email}");
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Preencha email e senha para login."));
            return;
        }
        if (Auth == null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Serviço de autenticação não inicializado."));
            return;
        }

        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    string firebaseErrorMsg = GetFirebaseError(task.Exception);
                    Debug.LogError($"❌ Erro no login: {firebaseErrorMsg}");

                    // Lógica para sobrescrever mensagens de erro (mantida conforme seu código original)
                    if (firebaseErrorMsg == "Email inválido.")
                    {
                        Debug.Log($"DISPARANDO ON_AUTH_FAILED com mensagem: {firebaseErrorMsg}");
                        OnAuthFailed?.Invoke(firebaseErrorMsg);
                    }
                    else
                    {
                        Debug.Log($"DISPARANDO ON_AUTH_FAILED com mensagem: E-mail e ou senha incorretos (original: {firebaseErrorMsg})");
                        OnAuthFailed?.Invoke("E-mail e ou senha incorretos");
                    }
                }
                else
                {
                    User = task.Result.User;
                    Debug.Log($"✅ Usuário logado: {User.Email}");
                    // OnAuthSuccess será disparado pelo AuthStateChanged, que é chamado após a autenticação.
                    // Não precisamos chamar OnAuthSuccess aqui diretamente, AuthStateChanged já cuida disso.
                }
            });
        });
    }

    public void Logout()
    {
        Debug.Log("FirebaseManager: Tentando fazer logout.");
        if (Auth != null && User != null)
        {
            Auth.SignOut();
            User = null;
            Debug.Log("✅ Usuário deslogado.");
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnLogout?.Invoke()); // Garante que o evento é disparado no thread principal
        }
        else
        {
            Debug.LogWarning("⚠️ Logout chamado sem usuário logado.");
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnLogout?.Invoke()); // Garante que o evento é disparado no thread principal
        }
    }

    public void SendPasswordResetEmail(string email)
    {
        Debug.Log($"FirebaseManager: Tentando enviar email de reset para: {email}");
        if (string.IsNullOrEmpty(email))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Digite o email para resetar a senha."));
            return;
        }
        if (Auth == null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnAuthFailed?.Invoke("Serviço de autenticação não inicializado."));
            return;
        }

        Auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    string erro = GetFirebaseError(task.Exception);
                    Debug.LogError($"❌ Erro ao enviar email de reset: {erro}");
                    OnAuthFailed?.Invoke(erro);
                }
                else
                {
                    Debug.Log("✅ Email de redefinição enviado.");
                    OnAuthFailed?.Invoke("Email de redefinição enviado. Verifique sua caixa de entrada."); // Usando OnAuthFailed para feedback de sucesso aqui
                }
            });
        });
    }

    private string GetFirebaseError(AggregateException exception)
    {
        if (exception == null) return "Erro desconhecido.";

        foreach (var e in exception.Flatten().InnerExceptions)
        {
            if (e is FirebaseException firebaseEx)
            {
                AuthError code = (AuthError)firebaseEx.ErrorCode;
                Debug.LogError($"Firebase Error Code: {code} | Message: {firebaseEx.Message}");

                switch (code)
                {
                    case AuthError.MissingEmail: return "Digite o email.";
                    case AuthError.InvalidEmail: return "Email inválido.";
                    case AuthError.WrongPassword: return "Senha incorreta.";
                    case AuthError.UserNotFound: return "Email não encontrado.";
                    case AuthError.EmailAlreadyInUse: return "Email já em uso.";
                    case AuthError.WeakPassword: return "Senha muito fraca.";
                    case AuthError.TooManyRequests: return "Muitas tentativas, tente mais tarde.";
                    case AuthError.NetworkRequestFailed: return "Erro de rede, verifique sua internet.";
                    case AuthError.UserDisabled: return "Conta desativada.";
                    case AuthError.AppNotAuthorized: return "Aplicação não autorizada no Firebase. Verifique as credenciais.";
                    // A mensagem restante é um fallback genérico ou para erros não mapeados.
                    default: return "Erro de autenticação. Tente novamente."; 
                }
            }
        }
        return "Um erro inesperado ocorreu.";
    }
}