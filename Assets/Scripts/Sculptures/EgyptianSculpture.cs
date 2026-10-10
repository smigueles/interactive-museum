using UnityEngine;

/// <summary>
/// Escultura egipcia. Scope propio: tipo de pose (de pie / sentada / momiforme),
/// dinastía, y nota sobre la convención del canon que ilustra esta pieza.
/// </summary>
public class EgyptianSculpture : Sculpture
{
    public enum PoseType { DePie, Sentada, Momiforme }

    [Header("Datos de Egipto")]
    [SerializeField] private PoseType pose;
    [SerializeField] private string dynasty;

    [SerializeField, TextArea(2, 4)]
    private string canonNote; // ej: nota sobre la ley de la frontalidad, jerarquía por escala, etc.

    public override string GetTooltip()
    {
        string poseInfo = pose switch
        {
            PoseType.DePie => "De pie, pierna adelantada — canon de marcha",
            PoseType.Sentada => "Entronizada — registro de poder estático",
            PoseType.Momiforme => "Momiforme — cuerpo eterno, fuera del tiempo",
            _ => ""
        };

        return $"{base.GetTooltip()}\n<i>{dynasty}</i>\n{poseInfo}\n{canonNote}";
    }
}