# Reflexión técnica · Práctica 2 (máximo una página)

> Plantilla para el PDF. Reemplazar los textos entre corchetes con lo que realmente pasó. Mantener todo en una página.

## 1. ¿Cómo abordaron el proceso de despliegue?

Seguimos un camino incremental: primero la API funcionando en local con Swagger, luego la imagen Docker y por último el despliegue en Kubernetes con la misma imagen. Cada etapa se validó antes de pasar a la siguiente (`dotnet run` → `docker run` → `kubectl apply`). [Agregar detalles propios: cómo se organizaron, qué videos siguieron, en qué equipos probaron.]

## 2. ¿Qué errores encontraron y cómo los resolvieron?

| Error | Causa | Solución |
|---|---|---|
| [Ej.: Swagger no cargaba dentro del contenedor] | [El template solo lo activa en Development y el contenedor corre como Production] | [Se habilitó Swagger en todos los ambientes en Program.cs] |
| [Ej.: pod en ImagePullBackOff] | [...] | [...] |
| [...] | [...] | [...] |

## 3. ¿Cómo se distribuyeron las responsabilidades del equipo?

| Integrante | Responsabilidad |
|---|---|
| Juan Andrés | API REST (CRUD de productos y Swagger) |
| Heyner | Dockerfile, construcción de la imagen y evidencias de Docker |
| Oscar Alexis | Manifiestos de Kubernetes y evidencias del clúster |
| Camilo | Repositorio, README y acceso del docente |
| Juliana | Guion y publicación del video, consolidación del PDF y entrega |

## 4. ¿Qué decisiones de la tecnología influyeron en el Dockerfile o el despliegue?

- **Build multi-etapa:** el SDK de .NET (~800 MB) solo se usa para compilar; la imagen final usa el runtime `aspnet:8.0`, mucho más liviana.
- **Restore en capa separada:** copiar primero el `.csproj` permite que Docker reutilice la caché de paquetes NuGet entre builds.
- **Puerto 8080:** desde .NET 8 las imágenes oficiales escuchan por defecto en 8080 (no en 80), por eso `EXPOSE 8080` y `containerPort: 8080`.
- **Usuario sin privilegios:** `USER $APP_UID`, incluido en las imágenes de .NET 8.
- **Endpoint `/health`:** permitió configurar `readinessProbe` y `livenessProbe`, y mostrar en el video qué pod responde.
- **Datos en memoria:** cada pod tiene su propia lista, así que los datos creados en un pod no se ven en el otro. Es aceptable para la práctica porque el objetivo es el despliegue; en producción se usaría una base de datos externa.
