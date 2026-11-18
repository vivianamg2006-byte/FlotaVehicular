# FlotaVehicular.Web

Frontend del sistema Flota Vehicular: login, sesión, rutas protegidas y una página principal provisional. No incluye menú, dashboard ni pantallas de módulos.

## Ejecución

Desde la raíz del repositorio, levanta primero la API:

```powershell
dotnet run --project FlotaVehicular.WebAPI --launch-profile http
```

En otra terminal, entra a `FlotaVehicular.Web` y ejecuta:

```powershell
npm install
copy .env.example .env
npm run dev
```

Abre `http://localhost:5173`. El servidor usa `strictPort`, así que ese puerto debe estar libre. El origen coincide con el CORS permitido por la API.

Para compilar y previsualizar:

```powershell
npm run build
npm run preview
```

## Configuración

`VITE_API_URL` define la URL base de la API; si no se define, se usa `http://localhost:5137`. El valor se normaliza sin barras finales en `src/config.js`.

## Usuarios de prueba

| Usuario | Contraseña | Rol |
| --- | --- | --- |
| `admin` | `Admin123!` | Administrador |
| `coordinador` | `Coord123!` | Coordinador/Técnico |
| `solicitante` | `Solic123!` | Solicitante/Consulta |

## Estructura

```text
src/
  api/
    client.js
    authApi.js
  auth/
    session.js
    AuthContext.jsx
    ProtectedRoute.jsx
  components/
    FleetScene.jsx
    vehicles.jsx
  pages/
    LoginPage.jsx
    HomePage.jsx
  styles/
    global.css
    login.css
  config.js
  App.jsx
  main.jsx
```

## Comportamiento del login

1. El paso 1 solicita solo el usuario o correo y valida que no esté vacío; no consulta la API para no revelar qué cuentas existen.
2. El paso 2 solicita la contraseña, permite mostrarla u ocultarla y envía las credenciales a `POST /api/auth/login`.
3. Mientras valida, el botón muestra `Validando...` con un spinner y los controles quedan deshabilitados. La flota avanza solo después de que la API confirma las credenciales y frena en cadena.
4. Un `401` se muestra como `Usuario o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.`; un `403` muestra el mensaje de la API, por ejemplo una cuenta desactivada.
5. Después del acceso se muestra la bienvenida y se navega a la página provisional.
6. La sesión se guarda en `sessionStorage` y en memoria. Cualquier `401` de la API cierra la sesión y deja el mensaje en el login; un temporizador cierra la sesión en la hora de expiración del token.
7. Las rutas protegidas redirigen a `/login` sin sesión y a `/` cuando el rol no está permitido.

Toda la información de usuario se obtiene de la API; no hay datos simulados ni fijos, salvo etiquetas de texto.
