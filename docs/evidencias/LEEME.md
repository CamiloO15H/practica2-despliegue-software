# Evidencias

Guardar aquí las capturas con estos nombres para que el PDF y el README las encuentren fácil.

## Docker (responsable: Heyner)

| Archivo | Qué debe verse |
|---|---|
| `01-docker-build.png` | Terminal con `docker build -t practica2-api:v1 .` terminado sin errores |
| `02-docker-images.png` | `docker images practica2-api` o la pestaña *Images* de Docker Desktop con la etiqueta `v1` |
| `03-docker-ps.png` | `docker ps` y la pestaña *Containers* de Docker Desktop con el contenedor en *Running* |
| `04-docker-swagger.png` | Navegador en `http://localhost:8080/swagger` ejecutando un GET con respuesta 200 |

## Kubernetes (responsable: Oscar Alexis)

| Archivo | Qué debe verse |
|---|---|
| `05-k8s-get-pods.png` | `kubectl get pods -n practica2` con los pods en *Running* |
| `06-k8s-get-svc.png` | `kubectl get svc -n practica2` con el NodePort 30080 |
| `07-k8s-swagger.png` | Navegador en `http://localhost:30080/swagger` ejecutando un GET con respuesta 200 |
| `08-k8s-yaml.png` | Los tres manifiestos YAML abiertos en el editor y legibles |

Tip: en la terminal, usar fuente grande y fondo claro para que los comandos se lean bien en el PDF.
