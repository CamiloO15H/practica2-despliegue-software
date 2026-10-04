# Práctica 2 · Despliegue de Software 2026-2

API REST de Productos en **.NET 8**, contenerizada con **Docker** y orquestada en **Kubernetes local (Docker Desktop)**.

---

## 👥 Integrantes

| Nombre completo | Responsabilidad |
| :--- | :--- |
| **CAMILO OSPINA HERNAN…** *(completar)* | Repositorio y README |
| **JUAN ANDRÉS RAM…** *(completar)* | API REST |
| **HEYNER MENA CA…** *(completar)* | Docker |
| **OSCAR ALEXIS PINEDA HENAO** | Kubernetes |
| **JULIANA ARENAS ARIAS** | Video, documento PDF y entrega |

---

## 🎥 Video de Demostración

▶️ **YouTube:** [Pegar aquí el enlace del video](https://youtube.com)

---

## 🛠️ Tecnología Utilizada

| Componente | Versión / Detalle |
| :--- | :--- |
| **Lenguaje y framework** | C# · ASP.NET Core Web API (.NET 8) |
| **Documentación de la API** | Swagger / OpenAPI (Swashbuckle) |
| **Almacenamiento** | En memoria (los datos se reinician al reiniciar el contenedor) |
| **Contenedores** | Docker Desktop |
| **Orquestación** | Kubernetes de Docker Desktop |
| **Imagen base** | `mcr.microsoft.com/dotnet/sdk:8.0` (build) y `mcr.microsoft.com/dotnet/aspnet:8.0` (runtime) |

---

## 📁 Estructura del Repositorio

```text
.
├── src/Practica2.Api/          # Código fuente de la API
│   ├── Controllers/            # ProductosController (CRUD)
│   ├── Data/                   # Repositorio en memoria
│   ├── Models/                 # Producto y ProductoRequest
│   └── Program.cs              # Configuración, Swagger y /health
├── k8s/
│   ├── 01-namespace.yaml       # Namespace practica2
│   ├── 02-deployment.yaml      # Deployment (2 réplicas, requests/limits, probes)
│   └── 03-service.yaml         # Service NodePort 30080
├── docs/                       # Reflexión técnica y evidencias
├── Dockerfile                  # Build multi-etapa
└── .dockerignore
```

---

## 📡 Endpoints de la API

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `GET` | `/api/productos` | Lista todos los productos |
| `GET` | `/api/productos/{id}` | Obtiene un producto por ID |
| `POST` | `/api/productos` | Crea un nuevo producto |
| `PUT` | `/api/productos/{id}` | Actualiza un producto existente |
| `DELETE` | `/api/productos/{id}` | Elimina un producto por ID |
| `GET` | `/health` | Estado de la API y nombre del contenedor/pod que respondió |
| `GET` | `/swagger` | Interfaz Swagger interactiva para pruebas |

### Ejemplo de cuerpo (*body*) para POST / PUT:
```json
{
  "nombre": "Audífonos",
  "categoria": "Audio",
  "precio": 120000,
  "stock": 10
}
```

---

## 🔌 Puertos Empleados

| Escenario | URL de prueba | Puerto host → puerto contenedor |
| :--- | :--- | :--- |
| **Local (`dotnet run`)** | `http://localhost:5257/swagger` | `5257` |
| **Docker** | `http://localhost:8080/swagger` | `8080` → `8080` |
| **Kubernetes (NodePort)** | `http://localhost:30080/swagger` | `30080` → Service `80` → Pod `8080` |
| **Kubernetes (port-forward, alternativa)** | `http://localhost:9090/swagger` | `9090` → Service `80` → Pod `8080` |

---

## 📋 Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) *(solo para ejecutar sin Docker)*
- **Docker Desktop** con Kubernetes habilitado:
  - *Settings* → *Kubernetes* → *Enable Kubernetes* (tipo de clúster `kubeadm`).
- Verificar que `kubectl` apunte a Docker Desktop:
  ```bash
  kubectl config use-context docker-desktop
  ```

---

## 🚀 Guía de Ejecución

### Parte 0 · Ejecutar en local (Opcional)

```bash
cd src/Practica2.Api
dotnet run
```
> Abrir en el navegador: `http://localhost:5257/swagger`

---

### Parte 1 · Docker

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
> Abrir en el navegador: `http://localhost:8080/swagger`

**Detener y eliminar el contenedor:**
```bash
docker stop practica2-api && docker rm practica2-api
```

---

### Parte 2 · Kubernetes

La misma imagen `practica2-api:v1` construida en la Parte 1 se utiliza en el clúster. Al usar `imagePullPolicy: IfNotPresent`, Kubernetes toma la imagen local y no la busca en Docker Hub.

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
> Abrir en el navegador: `http://localhost:30080/swagger`

**Alternativa con port-forward (si el NodePort no responde):**
```bash
kubectl port-forward svc/practica2-api-svc 9090:80 -n practica2
```
> Abrir en el navegador: `http://localhost:9090/swagger`

**Comandos útiles para la explicación/diagnóstico:**
```bash
kubectl get all -n practica2
kubectl describe deployment practica2-api -n practica2
kubectl logs -l app=practica2-api -n practica2
```

> [!TIP]
> Al llamar repetidamente a `/health`, el campo `host` alternará entre los dos pods, demostrando que el `Service` está balanceando la carga correctamente.

**Eliminar todos los recursos de Kubernetes:**
```bash
kubectl delete namespace practica2
```

---

## ✨ Buenas Prácticas Aplicadas

| Requisito | Dónde se cumple |
| :--- | :--- |
| **Namespace propio `practica2`** | `k8s/01-namespace.yaml` y declaración de `namespace:` en cada manifiesto. |
| **Requests y limits de CPU/memoria** | `k8s/02-deployment.yaml` en la sección `resources`. |
| **Imagen versionada + `IfNotPresent`** | `image: practica2-api:v1` y `imagePullPolicy: IfNotPresent`. |
| **Labels y selectors coherentes** | Etiqueta `app: practica2-api` en Deployment, template y Service. |
| **Extras** | Build multi-etapa, usuario sin privilegios, `readinessProbe` y `livenessProbe` sobre `/health`. |

---

## ❓ Problemas Frecuentes

| Síntoma | Causa | Solución |
| :--- | :--- | :--- |
| **Swagger no aparece en Docker** | En el template por defecto, Swagger solo se activa en `Development`. | Ya resuelto: `Program.cs` lo habilita en todos los ambientes. |
| **Pod en `ErrImagePull` / `ImagePullBackOff`** | El clúster no localiza la imagen local. | Construir la imagen antes de `kubectl apply`, verificar `imagePullPolicy: IfNotPresent` y que Kubernetes de Docker Desktop esté en modo `kubeadm`. |
| **`port is already allocated` al hacer `docker run`** | Otro contenedor u proceso ya ocupa el puerto 8080. | Ejecutar `docker rm -f practica2-api` o cambiar el puerto a `-p 8081:8080`. |
| **`localhost:30080` no responde** | El Service no localiza los pods. | Ejecutar `kubectl get endpoints -n practica2` y validar que las labels del Service coincidan con el Deployment. |
| **Cambié el código y Kubernetes no lo refleja** | Misma etiqueta `v1` cacheada. | Construir como `practica2-api:v2`, actualizar la imagen en el Deployment y aplicar con `kubectl apply -f k8s/`. |
