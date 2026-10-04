# Reflexión técnica · Práctica 2

## 1. ¿Cómo abordamos el proceso de despliegue?

Como equipo decidimos trabajar el despliegue por etapas para poder validar cada parte antes de continuar con la siguiente. Primero desarrollamos y probamos la API REST de productos en ASP.NET Core con .NET 8, verificando desde Swagger que las operaciones de consulta, creación, actualización y eliminación funcionaran correctamente.

Cuando confirmamos que la API funcionaba en local, pasamos a la parte de Docker. Construimos la imagen `practica2-api:v1`, ejecutamos el contenedor utilizando el puerto 8080 y validamos que la aplicación respondiera correctamente desde Swagger y desde el endpoint `/health`.

Después utilizamos esa misma imagen para realizar el despliegue en Kubernetes local. Creamos el namespace `practica2`, el Deployment con dos réplicas y el Service de tipo NodePort. Finalmente comprobamos que los pods estuvieran en estado Running y que la API fuera accesible desde `localhost:30080`.

Esta forma de trabajar nos permitió detectar cualquier problema en una etapa antes de avanzar a la siguiente y también facilitó la distribución de responsabilidades entre los integrantes.

## 2. ¿Qué errores encontramos y cómo los resolvimos?

Durante la práctica encontramos pocos inconvenientes, ya que gran parte de la configuración funcionó correctamente desde las primeras pruebas.

Uno de los inconvenientes se presentó al ejecutar localmente la API en el equipo de Juan. El proyecto está desarrollado en .NET 8, pero en su computador tenía instalado .NET 10. Para poder ejecutar la aplicación localmente se utilizó la opción de roll-forward al runtime disponible. Esto no afectó posteriormente el trabajo con Docker, porque la imagen utiliza directamente .NET 8.

También tuvimos un error al ejecutar inicialmente el comando de construcción de Docker. Se escribió `docker build -t practica2-api:v1` sin incluir el punto final, por lo que Docker indicó que faltaba especificar el contexto de construcción. Se corrigió utilizando `docker build -t practica2-api:v1 .` y la imagen se generó correctamente.

En las pruebas realizadas por Heyner, Oscar Alexis y Camilo no se presentaron inconvenientes adicionales. La construcción de la imagen, los manifiestos de Kubernetes y la configuración del repositorio funcionaron correctamente durante sus respectivas validaciones.

## 3. ¿Cómo distribuimos las responsabilidades del equipo?

Para organizarnos mejor, dividimos la práctica de acuerdo con las diferentes etapas del despliegue.

Juan Andrés se encargó del desarrollo de la API REST de productos, incluyendo las operaciones CRUD y la configuración de Swagger.

Heyner trabajó con el Dockerfile, la construcción de la imagen `practica2-api:v1`, la ejecución del contenedor y la validación del endpoint `/health`.

Oscar Alexis se encargó de la parte de Kubernetes, incluyendo los manifiestos, el namespace, el Deployment, el Service, la validación de los pods y el acceso a la API mediante NodePort.

Camilo se encargó de organizar el repositorio, actualizar el README y documentar los comandos, los puertos utilizados y la información necesaria para reproducir el despliegue.

Por mi parte (Juliana), me encargué de validar la ejecución de la API en Docker, organizar el video grupal, consolidar la información y las evidencias en el PDF y realizar la entrega final.

## 4. ¿Qué decisiones de la tecnología influyeron en el Dockerfile o en el despliegue?

Al trabajar con .NET 8, varias decisiones de configuración estuvieron relacionadas directamente con esta tecnología. Utilizamos un Dockerfile multi-etapa, donde primero usamos la imagen `sdk:8.0` para compilar y publicar la aplicación y posteriormente la imagen `aspnet:8.0` para ejecutar únicamente los archivos necesarios. Esto permite que la imagen final sea más liviana.

También se decidió copiar primero el archivo `.csproj` antes del resto del código para aprovechar la caché de Docker durante la restauración de paquetes.

La aplicación fue configurada para escuchar por el puerto 8080, por lo que este mismo puerto se utilizó dentro del contenedor y como `containerPort` en Kubernetes.

Para el despliegue utilizamos la imagen versionada `practica2-api:v1`, `imagePullPolicy: IfNotPresent`, dos réplicas y valores de requests y limits de CPU y memoria. También utilizamos el endpoint `/health` para las validaciones de disponibilidad.

Finalmente, la API utiliza datos almacenados en memoria. Para los objetivos de esta práctica fue suficiente, ya que el enfoque estaba en el proceso de despliegue. Sin embargo, entendimos que en una aplicación real sería necesario utilizar una base de datos externa para que todas las réplicas compartan y conserven la misma información.