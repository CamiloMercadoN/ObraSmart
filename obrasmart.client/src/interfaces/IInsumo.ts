export interface IInsumo {
  id?: string | null;
  tipoInsumo: string; // "Material" | "Mano de Obra" | "Equipo"
  descripcion: string;
  precioReferencia: number;
  unidadMedidaId: number | null;
  unidadMedidaNombre?: string;
  esPlantilla?: boolean;
  etiquetasIds: string[];
}

export interface IUnidadMedida {
  id: number;
  nombre: string;
  abreviacion: string;
}

export interface IEtiqueta {
  id: string;
  nombre: string;
  colorHex: string;
  esPlantilla: boolean;
}
