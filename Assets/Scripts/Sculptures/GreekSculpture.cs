using UnityEngine;

/// <summary>
/// Escultura griega. Scope propio: tipo de pose (kouros rígido vs. contrapposto)
/// y el escultor/canon asociado (ej: Policleto).
/// </summary>
public class GreekSculpture : Sculpture
{
    public enum PoseType { Kouros, Contrapposto, EnAccion }

    [Header("Datos de Grecia")]
    [SerializeField] private PoseType pose;
    [SerializeField] private string sculptorOrCanon; // ej: "Policleto - Doríforo"

    [SerializeField, TextArea(2, 4)]
    private string canonNote; // nota sobre la ruptura/continuidad del canon que ilustra esta pieza

    public override string GetTooltip()
    {
        string poseInfo = pose switch
        {
            PoseType.Kouros => "Postura rígida y frontal — herencia egipcia",
            PoseType.Contrapposto => "Contrapposto — peso en una pierna, tensión y movimiento",
            PoseType.EnAccion => "Cuerpo en pleno movimiento — captura un instante, no una pose",
            _ => ""
        };

        return $"{base.GetTooltip()}\n<i>{sculptorOrCanon}</i>\n{poseInfo}\n{canonNote}";
    }
}