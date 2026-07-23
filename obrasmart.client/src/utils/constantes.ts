export const TIPOS_INSUMO = [
  'Material',
  'Mano de Obra',
  'Equipo',
  'Servicio' 
];

const CLASES_POR_TIPO: Record<string, string> = {
  'Material': 'text-xs font-bold text-orange-600 bg-orange-50 border-round px-2 py-1',
  'Mano de Obra': 'text-xs font-bold text-blue-600 bg-blue-50 border-round px-2 py-1',
  'Equipo': 'text-xs font-bold text-purple-600 bg-purple-50 border-round px-2 py-1',
  'Servicio': 'text-xs font-bold text-green-600 bg-green-50 border-round px-2 py-1'
};

export const OBTENER_CLASE_TIPO_INSUMO = (tipo: string): string => {
  return CLASES_POR_TIPO[tipo] || 'text-xs font-bold text-500 bg-100 border-round px-2 py-1';
};
