# Práctica 2 · Despliegue de Software 2026-2

API REST de **Productos** en .NET 8, contenerizada con **Docker** y orquestada en **Kubernetes local** (Docker Desktop).

## Integrantes

| Nombre completo | Responsabilidad |
|---|---|
| CAMILO OSPINA HERNAN… *(completar)* | Repositorio y README |
| JUAN ANDRÉS RAMIREZ CASTAÑEDA| API REST |
| HEYNER MENA CA… *(completar)* | Docker |
| OSCAR ALEXIS PINE… *(completar)* | Kubernetes |
| JULIANA ARENAS ARIAS | Video, documento PDF y entrega |

## Video

▶️ **YouTube:** _pegar aquí el enlace del video_

## Tecnología utilizada

| Componente | Versión / detalle |
|---|---|
| Lenguaje y framework | C# · ASP.NET Core Web API (.NET 8) |
| Documentación de la API | Swagger / OpenAPI (Swashbuckle) |
| Almacenamiento | En memoria (los datos se reinician al reiniciar el contenedor) |
| Contenedores | Docker Desktop |
| Orquestación | Kubernetes de Docker Desktop |
| Imagen base | `mcr.microsoft.com/dotnet/sdk:8.0` (build) y `mcr.microsoft.com/dotnet/aspnet:8.0` (runtime) |

## Estructura del repositorio

```
.
├── src/Practica2.Api/          Código fuente de la API
│   ├── Controllers/            ProductosController (CRUD)
│   ├── Data/                   Repositorio en memoria
│   ├── Models/                 Producto y ProductoRequest
│   └── Program.cs              Configuración, Swagger y /health
├── k8s/
│   ├── 01-namespace.yaml       Namespace practica2
│   ├── 02-deployment.yaml      Deployment (2 réplicas, requests/limits, probes)
│   └── 03-service.yaml         Service NodePort 30080
├── docs/                       Reflexión técnica y evidencias
├── Dockerfile                  Build multi-etapa
└── .dockerignore
```

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos` | Lista todos los productos |
| GET | `/api/productos/{id}` | Obtiene un producto |
| POST | `/api/productos` | Crea un producto |
| PUT | `/api/productos/{id}` | Actualiza un producto |
| DELETE | `/api/productos/{id}` | Elimina un producto |
| GET | `/health` | Estado de la API y nombre del contenedor/pod que respondió |
| GET | `/swagger` | Interfaz Swagger para probar la API |

Ejemplo de cuerpo para POST/PUT:

```json
{ "nombre": "Audífonos", "categoria": "Audio", "precio": 120000, "stock": 10 }
```

## Puertos empleados

| Escenario | URL de prueba | Puerto host → puerto contenedor |
|---|---|---|
| Local (`dotnet run`) | http://localhost:5257/swagger | 5257 |
| Docker | http://localhost:8080/swagger | 8080 → 8080 |
| Kubernetes (NodePort) | http://localhost:30080/swagger | 30080 → Service 80 → pod 8080 |
| Kubernetes (port-forward, alternativa) | http://localhost:9090/swagger | 9090 → Service 80 → pod 8080 |

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (solo para ejecutar sin Docker)
- Docker Desktop con **Kubernetes habilitado**: *Settings → Kubernetes → Enable Kubernetes* (tipo de clúster **kubeadm**)
- Verificar que `kubectl` apunte a Docker Desktop:
  ```bash
  kubectl config use-context docker-desktop
  ```

## Parte 0 · Ejecutar en local (opcional)

```bash
cd src/Practica2.Api
dotnet run
```
Abrir http://localhost:5257/swagger

## Parte 1 · Docker

Desde la raíz del repositorio (donde está el `Dockerfile`):

```bash
# 1. Construir la imagen con etiqueta de versión
docker build -t practica2-api:v1 .

# 2. Verificar que la imagen existe
docker images practica2-api

# 3. Ejecutar el contenedor publicando el puerto 8080
docker run -d -p 8080:8080 --name practica2-api practica2-api:v1

# 4. Verificar que el contenedor está corriendo
docker ps --filter name=practica2-api

# 5. Probar la API
curl http://localhost:8080/health
curl http://localhost:8080/api/productos
```

Abrir http://localhost:8080/swagger en el navegador.

Para detener y eliminar el contenedor:

```bash
docker stop practica2-api && docker rm practica2-api
```

## Parte 2 · Kubernetes

La misma imagen `practica2-api:v1` construida en la Parte 1 se usa en el clúster. Como `imagePullPolicy` es `IfNotPresent`, Kubernetes toma la imagen local y no la busca en Docker Hub.

```bash
# 1. Aplicar los manifiestos (namespace, deployment y service, en orden)
kubectl apply -f k8s/

# 2. Verificar pods (deben quedar en estado Running)
kubectl get pods -n practica2

# 3. Verificar el service
kubectl get svc -n practica2

# 4. Probar la API a través del NodePort
curl http://localhost:30080/health
curl http://localhost:30080/api/productos
```

Abrir http://localhost:30080/swagger en el navegador.

**Alternativa con port-forward** (si el NodePort no responde):

```bash
kubectl port-forward svc/practica2-api-svc 9090:80 -n practica2
```
Abrir http://localhost:9090/swagger

**Comandos útiles para la explicación:**

```bash
kubectl get all -n practica2
kubectl describe deployment practica2-api -n practica2
kubectl logs -l app=practica2-api -n practica2
```

Al llamar varias veces a `/health`, el campo `host` cambia entre los dos pods: el Service está balanceando la carga.

Para eliminar todo:

```bash
kubectl delete namespace practica2
```

## Buenas prácticas aplicadas

| Requisito | Dónde se cumple |
|---|---|
| Namespace propio `practica2` | `k8s/01-namespace.yaml` y `namespace:` en cada recurso |
| Requests y limits de CPU/memoria | `k8s/02-deployment.yaml` → `resources` |
| Imagen versionada + `IfNotPresent` | `image: practica2-api:v1` y `imagePullPolicy: IfNotPresent` |
| Labels y selectors coherentes | `app: practica2-api` en Deployment, template y Service |
| Extras | Build multi-etapa, usuario sin privilegios, `readinessProbe` y `livenessProbe` sobre `/health` |

## Problemas frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| Swagger no aparece en Docker | En el template, Swagger solo se activa en `Development` | Ya resuelto: `Program.cs` lo habilita en todos los ambientes |
| Pod en `ErrImagePull` / `ImagePullBackOff` | El clúster no ve la imagen local | Construir la imagen antes de `kubectl apply`, verificar `imagePullPolicy: IfNotPresent` y que Kubernetes de Docker Desktop esté en modo **kubeadm** |
| `port is already allocated` al hacer `docker run` | Otro contenedor ya usa el 8080 | `docker rm -f practica2-api` o usar `-p 8081:8080` |
| `localhost:30080` no responde | El Service no encuentra pods | `kubectl get endpoints -n practica2` y revisar que las labels coincidan |
| Cambié el código y Kubernetes no lo refleja | Misma etiqueta `v1` en caché | Construir `practica2-api:v2`, actualizar la imagen en el Deployment y `kubectl apply -f k8s/` |
