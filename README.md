# Práctica 2 · Despliegue de Software 2026-2

## Los Indesplegables

API REST de Productos desarrollada en **ASP.NET Core con .NET 8**, contenerizada con **Docker** y desplegada en **Kubernetes local mediante Docker Desktop**.

---

## Integrantes

| Nombre completo | Responsabilidad |
| --- | --- |
| **Juliana Arenas Arias** | Validación en Docker, video, documento PDF y entrega |
| **Juan Andres Ramirez Castañeda** | Desarrollo de la API REST |
| **Heyner Mena Campaña** | Docker |
| **Oscar Alexis Pineda Henao** | Kubernetes |
| **Camilo Ospina Hernández** | Repositorio y README |

---

## Video de demostración

**YouTube:** [Práctica 2 - Los Indesplegables](https://youtu.be/Y_YmG6hdUBo)

---

## Tecnología utilizada

| Componente | Versión / Detalle |
| --- | --- |
| **Lenguaje** | C# |
| **Framework** | ASP.NET Core Web API (.NET 8) |
| **Documentación de la API** | Swagger / OpenAPI |
| **Almacenamiento** | En memoria |
| **Contenedores** | Docker Desktop |
| **Orquestación** | Kubernetes local mediante Docker Desktop |
| **Imagen de compilación** | `mcr.microsoft.com/dotnet/sdk:8.0` |
| **Imagen de ejecución** | `mcr.microsoft.com/dotnet/aspnet:8.0` |

Los datos de la API se almacenan en memoria, por lo que se reinician cuando se vuelve a ejecutar la aplicación o el contenedor.

---

## Estructura del repositorio

```text
.
├── src/
│   └── Practica2.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── Models/
│       └── Program.cs
│
├── k8s/
│   ├── 01-namespace.yaml
│   ├── 02-deployment.yaml
│   └── 03-service.yaml
│
├── docs/
├── Dockerfile
└── .dockerignore
```

---

## Endpoints de la API

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/Productos` | Lista todos los productos |
| `GET` | `/api/Productos/{id}` | Obtiene un producto por ID |
| `POST` | `/api/Productos` | Crea un nuevo producto |
| `PUT` | `/api/Productos/{id}` | Actualiza un producto existente |
| `DELETE` | `/api/Productos/{id}` | Elimina un producto por ID |
| `GET` | `/health` | Consulta el estado de la API y el host que respondió |
| `GET` | `/swagger` | Interfaz Swagger para probar los endpoints |

### Ejemplo de cuerpo para POST / PUT

```json
{
  "nombre": "Audífonos",
  "categoria": "Audio",
  "precio": 120000,
  "stock": 10
}
```

---

## Puertos empleados

| Escenario | URL de prueba | Configuración |
| --- | --- | --- |
| **Ejecución local** | `http://localhost:5257/swagger` | Puerto `5257` |
| **Docker** | `http://localhost:8080/swagger` | Host `8080` → Contenedor `8080` |
| **Kubernetes - NodePort** | `http://localhost:30080/swagger` | Host `30080` → Service `80` → Pod `8080` |
| **Kubernetes - port-forward** | `http://localhost:9090/swagger` | Host `9090` → Service `80` → Pod `8080` |

---

## Requisitos previos

Para reproducir el despliegue se requiere:

- Docker Desktop.
- Kubernetes habilitado en Docker Desktop.
- `kubectl`.
- .NET 8 SDK únicamente si se desea ejecutar la API directamente sin Docker.

Para verificar que `kubectl` esté utilizando Kubernetes de Docker Desktop:

```bash
kubectl config use-context docker-desktop
```

---

## Guía de ejecución

### Parte 0 · Ejecutar la API localmente

Esta parte es opcional y permite validar la API antes de utilizar Docker.

```bash
cd src/Practica2.Api
dotnet run
```

Abrir en el navegador:

```text
http://localhost:5257/swagger
```

---

### Parte 1 · Docker

Los siguientes comandos deben ejecutarse desde la raíz del repositorio, donde se encuentra el `Dockerfile`.

#### 1. Construir la imagen

```bash
docker build -t practica2-api:v1 .
```

#### 2. Verificar la imagen

```bash
docker images practica2-api
```

Debe aparecer la imagen:

```text
practica2-api:v1
```

#### 3. Ejecutar el contenedor

```bash
docker run -d -p 8080:8080 --name practica2-api practica2-api:v1
```

#### 4. Verificar el contenedor

```bash
docker ps --filter name=practica2-api
```

#### 5. Validar la API

```bash
curl http://localhost:8080/health
```

También se puede consultar el listado de productos:

```bash
curl http://localhost:8080/api/Productos
```

Swagger se encuentra disponible en:

```text
http://localhost:8080/swagger
```

#### Detener y eliminar el contenedor

```bash
docker stop practica2-api
docker rm practica2-api
```

---

### Parte 2 · Kubernetes

Para Kubernetes se utiliza la misma imagen Docker:

```text
practica2-api:v1
```

El Deployment está configurado con:

```yaml
imagePullPolicy: IfNotPresent
```

#### 1. Aplicar los manifiestos

```bash
kubectl apply -f k8s/
```

Este comando crea:

- Namespace `practica2`.
- Deployment `practica2-api`.
- Service `practica2-api-svc`.

#### 2. Verificar los pods

```bash
kubectl get pods -n practica2
```

El Deployment está configurado con **2 réplicas**, por lo que deben aparecer dos pods en estado `Running`.

#### 3. Verificar el Service

```bash
kubectl get svc -n practica2
```

El Service utiliza el tipo `NodePort` y publica la API mediante el puerto `30080`.

#### 4. Probar la API en Kubernetes

```bash
curl http://localhost:30080/health
```

También se puede consultar:

```bash
curl http://localhost:30080/api/Productos
```

Swagger está disponible en:

```text
http://localhost:30080/swagger
```

---

## Verificación de las réplicas

Al ejecutar varias veces:

```bash
curl http://localhost:30080/health
```

el campo `host` puede cambiar entre los diferentes pods disponibles.

Esto permite observar cómo el Service distribuye las solicitudes entre las réplicas de la aplicación.

---

## Alternativa con port-forward

También es posible acceder a la API mediante `port-forward`:

```bash
kubectl port-forward svc/practica2-api-svc 9090:80 -n practica2
```

Después se puede abrir:

```text
http://localhost:9090/swagger
```

---

## Comandos útiles de validación

Ver todos los recursos:

```bash
kubectl get all -n practica2
```

Consultar el Deployment:

```bash
kubectl describe deployment practica2-api -n practica2
```

Consultar los logs:

```bash
kubectl logs -l app=practica2-api -n practica2
```

Eliminar los recursos de Kubernetes:

```bash
kubectl delete namespace practica2
```

---

## Buenas prácticas aplicadas

| Requisito | Implementación |
| --- | --- |
| **Namespace propio** | Se utiliza el namespace `practica2`. |
| **Imagen versionada** | Se utiliza `practica2-api:v1`. |
| **Política de imagen** | `imagePullPolicy: IfNotPresent`. |
| **Réplicas** | Deployment configurado con 2 réplicas. |
| **Requests y limits** | Se definen valores de CPU y memoria. |
| **Labels y selectors** | Se utiliza `app: practica2-api` en Deployment y Service. |
| **Readiness Probe** | Se utiliza el endpoint `/health`. |
| **Liveness Probe** | Se utiliza el endpoint `/health`. |
| **Docker multi-etapa** | Se separa la compilación de la ejecución de la aplicación. |
| **Usuario sin privilegios** | Se utiliza `USER $APP_UID`. |
| **Health endpoint** | `/health` permite validar la aplicación e identificar el pod que respondió. |

---

## Posibles problemas y diagnóstico

Los siguientes casos se incluyen como referencia para facilitar la reproducción del despliegue.

| Situación | Posible causa | Solución |
| --- | --- | --- |
| **Swagger no aparece dentro de Docker** | Swagger puede estar configurado únicamente para Development. | Verificar que Swagger esté habilitado también cuando la aplicación se ejecuta en Production. |
| **`ErrImagePull` o `ImagePullBackOff`** | Kubernetes no encuentra la imagen. | Verificar que exista `practica2-api:v1` y que se utilice `imagePullPolicy: IfNotPresent`. |
| **`port is already allocated`** | Otro contenedor está utilizando el puerto 8080. | Ejecutar `docker rm -f practica2-api` o utilizar otro puerto del host. |
| **`localhost:30080` no responde** | El Service no encuentra correctamente los pods. | Revisar los pods, labels y selectors del Deployment y del Service. |
| **Los cambios del código no aparecen** | Kubernetes continúa utilizando una imagen anterior. | Crear una nueva versión de la imagen y actualizar el Deployment. |

---

## Evidencias

Las evidencias del proceso se encuentran almacenadas en:

```text
docs/evidencias/
```

Las evidencias incluyen:

- API ejecutándose localmente.
- Construcción de la imagen Docker.
- Imagen `practica2-api:v1`.
- Contenedor Docker en ejecución.
- Swagger ejecutándose desde Docker.
- Pods de Kubernetes en estado `Running`.
- Service de Kubernetes.
- Swagger ejecutándose desde Kubernetes.
- Manifiestos YAML de Namespace, Deployment y Service.

---

## Acceso al repositorio

El docente debe tener acceso al repositorio mediante el usuario de GitHub:

```text
oalarconpe
```

---

## Video final

La demostración completa del proceso se encuentra disponible en YouTube:

[Práctica 2 · Docker y Kubernetes · Los Indesplegables](https://youtu.be/Y_YmG6hdUBo)