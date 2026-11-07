# 01 · Física 2D, Input, Colisiones y Aparición de Objetos

> Teoría para el juego 1 (**FlappyClone**). Incluye todo lo del tema, aunque el juego no lo use todo.
> Versión de Unity: **Unity 6** (algunos nombres cambiaron respecto a versiones anteriores; se indica cuando aplica).

---

## Índice

1. [Rigidbody2D: el motor de física](#1-rigidbody2d-el-motor-de-física)
2. [Update vs FixedUpdate](#2-update-vs-fixedupdate)
3. [GetComponent: hablar con otros components](#3-getcomponent-hablar-con-otros-components)
4. [Input: leer teclado, mouse y pantalla táctil](#4-input-leer-teclado-mouse-y-pantalla-táctil)
5. [Colliders 2D](#5-colliders-2d)
6. [Colisiones vs Triggers](#6-colisiones-vs-triggers)
7. [Tags y Layers](#7-tags-y-layers)
8. [Instantiate y Destroy: crear y borrar objetos](#8-instantiate-y-destroy-crear-y-borrar-objetos)
9. [Temporizadores](#9-temporizadores)
10. [Números aleatorios](#10-números-aleatorios)
11. [Rotación y utilidades de Mathf](#11-rotación-y-utilidades-de-mathf)
12. [Reiniciar la escena y pausar](#12-reiniciar-la-escena-y-pausar)
13. [Errores comunes](#13-errores-comunes)

---

## 1. Rigidbody2D: el motor de física

Sin Rigidbody2D, un objeto solo se mueve si **tú** cambias su `transform`. Con Rigidbody2D, **el motor de física de Unity** lo controla: gravedad, empujones, velocidad, choques.

> **Analogía:** mover con `transform` es teletransportar el objeto un poquito cada frame. Usar Rigidbody2D es lanzarlo como una pelota real y dejar que la física decida su trayectoria.

### Body Type (tipo de cuerpo)

| Tipo | Le afecta la gravedad | Lo empujan otros objetos | Uso típico |
|---|---|---|---|
| **Dynamic** | Sí | Sí | Jugador, pelotas, cajas que caen |
| **Kinematic** | No | No (tú lo mueves por código) | Plataformas móviles, enemigos con ruta fija |
| **Static** | No | No, nunca se mueve | Suelo, paredes |

### Propiedades en el Inspector

| Propiedad | Qué hace |
|---|---|
| **Mass** | Masa. Más masa = cuesta más empujarlo. |
| **Linear Damping** | "Resistencia del aire" para el movimiento. Frena el objeto con el tiempo. *(Antes de Unity 6 se llamaba `Drag`.)* |
| **Angular Damping** | Lo mismo, pero para el giro. *(Antes `Angular Drag`.)* |
| **Gravity Scale** | Multiplicador de la gravedad. `1` normal, `0` sin gravedad, `2` cae el doble de rápido. |
| **Collision Detection** | `Discrete` (normal) o `Continuous` (para objetos muy rápidos que atraviesan paredes). |
| **Interpolate** | Suaviza el movimiento visual. Útil en el jugador si se ve "tembloroso". |
| **Constraints** | Congela ejes. `Freeze Rotation Z` evita que el objeto gire al chocar. |

La gravedad global del proyecto se cambia en **Edit → Project Settings → Physics 2D → Gravity** (por defecto `(0, -9.81)`).

### Controlar el Rigidbody2D desde código

```csharp
private Rigidbody2D rb;

// Cambiar la velocidad directamente (Unity 6: linearVelocity; antes: velocity)
rb.linearVelocity = new Vector2(0f, 5f);          // sale hacia arriba a 5 u/s
rb.linearVelocity = Vector2.zero;                 // lo detiene en seco

// Leer la velocidad
float velocidadY = rb.linearVelocity.y;

// Aplicar fuerzas
rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse); // empujón instantáneo (salto)
rb.AddForce(Vector2.right * 2f, ForceMode2D.Force); // empuje continuo (como un motor)

// Mover un cuerpo Kinematic respetando la física
rb.MovePosition(rb.position + Vector2.right * 2f * Time.fixedDeltaTime);

// Otras
rb.gravityScale = 0f;              // apagar la gravedad por código
rb.angularVelocity = 0f;           // detener el giro
rb.bodyType = RigidbodyType2D.Kinematic;
```

**`Impulse` vs `Force`:**
- **Impulse:** golpe único, como patear una pelota. Ideal para saltos.
- **Force:** empuje sostenido, como un cohete. Se aplica cada frame de física.

**`Vector2` vs `Vector3`:** en física 2D se usa `Vector2` (x, y). Unity convierte entre ambos automáticamente cuando hace falta.

---

## 2. Update vs FixedUpdate

| Método | Cuándo se ejecuta | Para qué |
|---|---|---|
| **Update** | Una vez por frame (variable: 30, 60, 144 veces/seg) | **Leer input**, lógica general, temporizadores |
| **FixedUpdate** | A intervalos fijos (50 veces/seg por defecto) | **Física**: fuerzas, velocidad, `MovePosition` |

> **Regla de oro:** el input se **lee** en `Update` (si no, puedes perder pulsaciones), y la física se **aplica** en `FixedUpdate`.

Patrón típico: `Update` detecta el botón y guarda un `bool`; `FixedUpdate` lo consume.

```csharp
private bool quiereSaltar;

void Update()
{
    if (/* se presionó el botón */) quiereSaltar = true;
}

void FixedUpdate()
{
    if (quiereSaltar)
    {
        // aplicar la fuerza aquí
        quiereSaltar = false;
    }
}
```

**Excepción práctica:** un **impulso único** (`ForceMode2D.Impulse` o asignar `linearVelocity` una vez) aplicado directamente en `Update` funciona bien en juegos simples. Lo que **nunca** va en `Update` es una fuerza continua.

En `FixedUpdate` se usa **`Time.fixedDeltaTime`** en vez de `Time.deltaTime`.

---

## 3. GetComponent: hablar con otros components

Un script necesita una **referencia** para usar otro component del mismo GameObject.

```csharp
private Rigidbody2D rb;

void Awake()
{
    rb = GetComponent<Rigidbody2D>();   // busca el Rigidbody2D de ESTE objeto
}
```

- `<Rigidbody2D>` entre `< >` indica **qué tipo** de component buscar (esto se llama *genérico*).
- Si el objeto no tiene ese component, devuelve `null`.
- **Guárdalo en una variable una sola vez** (en `Awake` o `Start`). Llamar `GetComponent` en cada `Update` es lento.

**`Awake` vs `Start`:** ambos se ejecutan una vez al inicio. `Awake` va primero. Convención: en `Awake` obtienes **tus propias** referencias; en `Start` usas las de **otros** objetos (que ya hicieron su `Awake`).

### Obligar a que exista un component

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class Jugador : MonoBehaviour { }
```

Si agregas este script a un objeto, Unity le agrega el Rigidbody2D automáticamente y no deja quitarlo.

### Referencias a OTROS objetos

```csharp
[SerializeField] private Transform objetivo;  // la arrastras desde la Hierarchy al Inspector
```

---

## 4. Input: leer teclado, mouse y pantalla táctil

Unity 6 usa el **Input System** (el sistema nuevo). Tu proyecto ya lo trae activado. Requiere esta línea arriba del script:

```csharp
using UnityEngine.InputSystem;
```

### Lectura directa (la más simple)

```csharp
// Teclado
Keyboard.current.spaceKey.wasPressedThisFrame    // true SOLO el frame en que se presionó
Keyboard.current.spaceKey.isPressed              // true MIENTRAS esté presionada
Keyboard.current.spaceKey.wasReleasedThisFrame   // true el frame en que se soltó
Keyboard.current.aKey.isPressed                  // otras teclas: aKey, wKey, leftArrowKey, escapeKey...

// Mouse
Mouse.current.leftButton.wasPressedThisFrame
Mouse.current.position.ReadValue()               // posición del mouse en píxeles (Vector2)

// Pantalla táctil
Touchscreen.current.primaryTouch.press.wasPressedThisFrame

// Pointer: mouse Y táctil a la vez (ideal para juegos de PC + móvil)
Pointer.current.press.wasPressedThisFrame
```

| Propiedad | Cuándo es `true` | Ejemplo de uso |
|---|---|---|
| `wasPressedThisFrame` | Un solo frame, al presionar | Saltar, disparar |
| `isPressed` | Todo el tiempo que está presionado | Caminar, acelerar |
| `wasReleasedThisFrame` | Un solo frame, al soltar | Cargar un ataque y soltarlo |

**Cuidado:** `Keyboard.current` es `null` si no hay teclado (por ejemplo, en un celular). Para juegos multiplataforma, verifica antes:

```csharp
if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) { }
```

### Input Actions (el sistema completo)

Tu proyecto tiene un archivo `InputSystem_Actions`. En lugar de preguntar por teclas concretas, defines **acciones** ("Saltar", "Moverse") y les asignas varias teclas o botones (espacio, botón A del control, toque en pantalla). El código pregunta por la acción, no por la tecla. Es lo que usaremos en **FOTA**, porque permite controles configurables y varios jugadores. Para FlappyClone basta con la lectura directa.

### El sistema viejo (para que lo reconozcas)

En muchos tutoriales verás:

```csharp
Input.GetKeyDown(KeyCode.Space)   // equivale a wasPressedThisFrame
Input.GetKey(KeyCode.Space)       // equivale a isPressed
Input.GetMouseButtonDown(0)
Input.GetAxis("Horizontal")
```

En un proyecto nuevo de Unity 6 esto **da error** (*InvalidOperationException*), porque el sistema viejo está desactivado. Se puede activar en **Project Settings → Player → Active Input Handling**, pero lo correcto hoy es usar el nuevo.

---

## 5. Colliders 2D

Un **Collider** define la **forma física** del objeto: dónde es "sólido". El sprite es lo que se ve; el collider es lo que choca. No tienen que coincidir exactamente.

| Collider | Forma | Uso típico |
|---|---|---|
| **BoxCollider2D** | Rectángulo | Plataformas, cajas, tuberías |
| **CircleCollider2D** | Círculo | Pelotas, monedas, personajes redondos |
| **CapsuleCollider2D** | Cápsula | Personajes (no se traban en esquinas) |
| **PolygonCollider2D** | Se ajusta al contorno del sprite | Formas irregulares |
| **EdgeCollider2D** | Línea (abierta) | Terrenos, bordes del nivel |
| **CompositeCollider2D** | Une varios colliders en uno | Niveles hechos con Tilemaps |

### Propiedades comunes

| Propiedad | Qué hace |
|---|---|
| **Is Trigger** | Lo convierte en zona de detección (no es sólido). Ver sección 6. |
| **Offset** | Desplaza el collider respecto al centro del objeto. |
| **Size / Radius** | Tamaño del collider. |
| **Material** | Un **Physics Material 2D**: define **Friction** (fricción) y **Bounciness** (rebote, de 0 a 1). |

Se crea un material con **Create → 2D → Physics Material 2D**. Con `Bounciness = 1` y `Friction = 0`, un objeto rebota sin perder energía (útil para Pong).

En la ventana Scene, el collider se ve como un contorno verde. Puedes editarlo con el botón **Edit Collider** del Inspector.

---

## 6. Colisiones vs Triggers

| | **Colisión** (Is Trigger desmarcado) | **Trigger** (Is Trigger marcado) |
|---|---|---|
| ¿Es sólido? | Sí, los objetos se detienen o rebotan | No, se atraviesan |
| ¿Para qué? | Suelo, paredes, chocar con obstáculos | Zonas: monedas, meta, "pasaste la tubería" |

### Métodos que Unity llama solos

```csharp
// COLISIONES (recibes información del choque)
void OnCollisionEnter2D(Collision2D choque)  { }  // en el instante del choque
void OnCollisionStay2D(Collision2D choque)   { }  // cada frame mientras siguen en contacto
void OnCollisionExit2D(Collision2D choque)   { }  // cuando se separan

// TRIGGERS (recibes el collider que entró)
void OnTriggerEnter2D(Collider2D otro) { }  // cuando algo entra a la zona
void OnTriggerStay2D(Collider2D otro)  { }  // cada frame mientras está dentro
void OnTriggerExit2D(Collider2D otro)  { }  // cuando sale
```

Dentro de ellos sabes **con qué** chocaste:

```csharp
void OnCollisionEnter2D(Collision2D choque)
{
    Debug.Log("Choqué con " + choque.gameObject.name);
}

void OnTriggerEnter2D(Collider2D otro)
{
    Debug.Log("Entró " + otro.gameObject.name);
}
```

### ¿Cuándo se disparan?

Para que Unity detecte el contacto:
1. **Ambos** objetos necesitan un **Collider2D**.
2. **Al menos uno** necesita un **Rigidbody2D**.

| Objeto A | Objeto B | ¿Hay evento? |
|---|---|---|
| Collider + Rigidbody2D Dynamic | Collider solo | ✅ Sí |
| Collider + Rigidbody2D Dynamic | Collider + Rigidbody2D Dynamic | ✅ Sí |
| Collider solo | Collider solo | ❌ No |
| Collider + Rigidbody2D Kinematic | Collider solo | ⚠️ Solo triggers, no colisiones |

> **Atención a la firma:** es `OnCollisionEnter2D` con **2D** y con parámetro `Collision2D`. Si escribes `OnCollisionEnter` (sin 2D), Unity **no lo llama nunca** y no da ningún error. Es uno de los fallos más frustrantes.

---

## 7. Tags y Layers

### Tags (etiquetas)

Un **Tag** es una etiqueta de texto para identificar tipos de objetos ("Obstaculo", "Moneda", "Player"). Se asigna arriba del Inspector, en **Tag → Add Tag...**

```csharp
if (otro.CompareTag("Obstaculo"))
{
    // perdió
}
```

Usa **`CompareTag`** y no `otro.tag == "Obstaculo"`: es más rápido y te avisa si el tag no existe.

### Layers (capas)

Un **Layer** agrupa objetos para que la física y la cámara los traten distinto. Por ejemplo, que los enemigos no choquen entre sí. Se configura en **Project Settings → Physics 2D → Layer Collision Matrix**. Lo usaremos a fondo en FOTA.

---

## 8. Instantiate y Destroy: crear y borrar objetos

### Instantiate: crear copias de un Prefab durante el juego

```csharp
[SerializeField] private GameObject prefabObstaculo;  // se arrastra el Prefab al Inspector

void CrearObstaculo()
{
    Vector3 posicion = new Vector3(10f, 0f, 0f);
    Instantiate(prefabObstaculo, posicion, Quaternion.identity);
}
```

- **`Quaternion.identity`** significa "sin rotación".
- `Instantiate` **devuelve** el objeto creado, por si necesitas modificarlo:

```csharp
GameObject nuevo = Instantiate(prefabObstaculo, posicion, Quaternion.identity);
nuevo.name = "Obstaculo_" + contador;
```

### Destroy: borrar objetos

```csharp
Destroy(gameObject);            // se borra a sí mismo
Destroy(gameObject, 5f);        // se borra a sí mismo en 5 segundos
Destroy(otro.gameObject);       // borra otro objeto (ej. la moneda que tocaste)
Destroy(this);                  // borra SOLO este component, no el objeto
```

> **Importante:** todo lo que creas con `Instantiate` y sale de la pantalla debe **destruirse**, o se acumularán miles de objetos invisibles consumiendo memoria y rendimiento.

---

## 9. Temporizadores

### Opción A: acumular tiempo (la más usada y la más clara)

```csharp
[SerializeField] private float intervalo = 2f;
private float temporizador = 0f;

void Update()
{
    temporizador += Time.deltaTime;

    if (temporizador >= intervalo)
    {
        // hacer algo cada 2 segundos
        temporizador = 0f;
    }
}
```

### Opción B: InvokeRepeating

```csharp
void Start()
{
    InvokeRepeating("CrearObstaculo", 1f, 2f); // empieza en 1s, repite cada 2s
}

CancelInvoke("CrearObstaculo");  // para detenerlo
Invoke("Reiniciar", 3f);         // ejecutar UNA vez dentro de 3s
```

Simple, pero el nombre del método va como texto: si te equivocas al escribirlo, no hay error en el editor.

### Opción C: Corrutinas

Una **corrutina** es un método que puede "pausarse" y continuar después.

```csharp
using System.Collections;

void Start()
{
    StartCoroutine(GenerarObstaculos());
}

IEnumerator GenerarObstaculos()
{
    while (true)
    {
        CrearObstaculo();
        yield return new WaitForSeconds(2f); // espera 2s y sigue
    }
}
```

Muy útiles para secuencias ("espera 1s, muestra texto, espera 2s, reinicia"). Las veremos a fondo más adelante.

### Tiempos útiles

| Propiedad | Qué es |
|---|---|
| `Time.deltaTime` | Segundos desde el frame anterior |
| `Time.fixedDeltaTime` | Intervalo fijo de la física (0.02 por defecto) |
| `Time.time` | Segundos desde que empezó el juego |
| `Time.timeScale` | Velocidad del tiempo: `1` normal, `0` pausado, `0.5` cámara lenta |

---

## 10. Números aleatorios

```csharp
float alturaAleatoria = Random.Range(-2f, 2f);  // float: entre -2 y 2 (INCLUYE el 2)
int numero = Random.Range(0, 3);                // int: 0, 1 o 2 (NO incluye el 3)
float probabilidad = Random.value;              // float entre 0 y 1
```

> **Trampa clásica:** con enteros, el máximo **no se incluye**. `Random.Range(0, 3)` nunca da 3. Con floats sí se incluye.

---

## 11. Rotación y utilidades de Mathf

### Rotar en 2D

En 2D solo se gira sobre el eje **Z**.

```csharp
transform.rotation = Quaternion.Euler(0f, 0f, 30f);  // apunta a 30 grados
transform.Rotate(0f, 0f, 90f * Time.deltaTime);      // gira 90 grados por segundo
```

Unity guarda las rotaciones como **Quaternion** (una forma matemática que evita errores al girar en 3D). Tú casi siempre trabajas en grados con `Quaternion.Euler`.

### Mathf: funciones matemáticas

```csharp
Mathf.Clamp(valor, min, max)   // limita un valor: Clamp(15, 0, 10) → 10
Mathf.Abs(-5)                  // valor absoluto → 5
Mathf.Lerp(a, b, t)            // punto intermedio entre a y b (t de 0 a 1)
Mathf.Max(a, b) / Mathf.Min(a, b)
Mathf.Sign(-3)                 // signo → -1
Mathf.Round / Mathf.Floor / Mathf.Ceil
```

Ejemplo típico de un pájaro que se inclina según su velocidad vertical:

```csharp
float angulo = Mathf.Clamp(rb.linearVelocity.y * 5f, -60f, 30f);
transform.rotation = Quaternion.Euler(0f, 0f, angulo);
```

---

## 12. Reiniciar la escena y pausar

### Recargar la escena actual

```csharp
using UnityEngine.SceneManagement;

void Reiniciar()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
```

Para cargar escenas por código, deben estar agregadas en **File → Build Profiles → Scene List**. Si no están ahí, `LoadScene` da error.

Otras formas:

```csharp
SceneManager.LoadScene("NombreDeLaEscena");  // por nombre
SceneManager.LoadScene(0);                   // por índice en la Scene List
```

### Pausar

```csharp
Time.timeScale = 0f;  // congela física, deltaTime y animaciones
Time.timeScale = 1f;  // reanuda
```

> **Cuidado:** si recargas la escena con `timeScale = 0`, el juego **sigue congelado**, porque `timeScale` no se reinicia solo. Vuelve a ponerlo en `1` antes de recargar.

---

## 13. Errores comunes

| Síntoma | Causa probable |
|---|---|
| `OnCollisionEnter2D` / `OnTriggerEnter2D` nunca se ejecuta | Falta Rigidbody2D en alguno de los dos; o escribiste el método sin "2D"; o el trigger está marcado/desmarcado al revés |
| El objeto atraviesa las paredes | Va muy rápido: Collision Detection en `Continuous` |
| El personaje gira como loco al chocar | Falta `Freeze Rotation Z` en Constraints |
| `NullReferenceException` | Usas una variable que nunca recibió valor: un `GetComponent` que no encontró nada, o un campo del Inspector sin asignar |
| `InvalidOperationException` al leer input | Usaste `Input.GetKeyDown` (sistema viejo) en un proyecto con el Input System nuevo |
| El salto a veces no responde | Estás leyendo el input en `FixedUpdate` en vez de en `Update` |
| La escena no carga | No está en **Build Profiles → Scene List** |
| El juego se queda congelado al reiniciar | `Time.timeScale` quedó en 0 |
| El juego va más lento con el tiempo | Objetos creados con `Instantiate` que nunca se destruyen |
