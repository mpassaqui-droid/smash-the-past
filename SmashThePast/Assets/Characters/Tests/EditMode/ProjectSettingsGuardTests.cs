using NUnit.Framework;
using UnityEditor;

namespace SmashThePast.Characters.Tests
{
    // Guarda el ajuste que rompe el juego en silencio: si activeInputHandler vuelve
    // a 0 (solo Input Manager antiguo), la suite sigue en verde pero el player
    // construido desde un clon limpio se queda sin teclado ni mando.
    public class ProjectSettingsGuardTests
    {
        private const int InputManagerAntiguo = 0;

        [Test]
        public void ElBackendDelNuevoInputSystemEstaActivo()
        {
            var ajustes = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            Assert.IsNotNull(ajustes, "No se pudo leer ProjectSettings.asset");
            Assert.IsNotEmpty(ajustes, "ProjectSettings.asset no contiene ningun objeto");

            var serializado = new SerializedObject(ajustes[0]);
            var propiedad = serializado.FindProperty("activeInputHandler");
            Assert.IsNotNull(propiedad, "ProjectSettings.asset no expone activeInputHandler");
            Assert.AreNotEqual(
                InputManagerAntiguo,
                propiedad.intValue,
                "activeInputHandler esta en 0: el nuevo Input System queda desactivado en el build.");
        }

        [Test]
        public void ElCodigoSeCompilaConEnableInputSystem()
        {
#if ENABLE_INPUT_SYSTEM
            Assert.Pass();
#else
            Assert.Fail("ENABLE_INPUT_SYSTEM no esta definido: el backend nuevo no esta activo.");
#endif
        }
    }
}
