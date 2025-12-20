import sys
import json
import numpy as np
import math
from scipy.integrate import solve_ivp
from scipy.interpolate import interp1d
from datetime import datetime

# ==========================================
# 1. CROP PARAMETERS
# ==========================================

CROP_PARAMS = {
    "Corn": {
        "harvest_index": 0.50, "max_biomass": 15, "max_leaf_area": 6,
        "gdd_stages": {"emergence": 120, "vegetative": 600, "flowering": 1000, "grain_filling": 1300},
        "n_requirement": 150, "p_requirement": 60, "k_requirement": 120,
        "fc": 0.35, "pwp": 0.15, "swc_initial": 0.28, "root_depth": 1.2,
        "Kc_stages": {"sowing": 0.3, "emergence": 0.5, "vegetative": 0.8, "flowering": 1.15, "grain_filling": 0.9},
        "max_a": 120, "max_j": 200, "r_dark": 1.5,
        "season_days": 120, "planting_season": "Summer", "ndvi_initial": 0.3, "optimal_ph": (6.0, 7.0), "optimal_temp": (20, 30)
    },
    "Wheat": {
        "harvest_index": 0.45, "max_biomass": 12, "max_leaf_area": 5,
        "gdd_stages": {"emergence": 100, "vegetative": 500, "flowering": 900, "grain_filling": 1200},
        "n_requirement": 120, "p_requirement": 50, "k_requirement": 100,
        "fc": 0.33, "pwp": 0.13, "swc_initial": 0.26, "root_depth": 1.0,
        "Kc_stages": {"sowing": 0.3, "emergence": 0.5, "vegetative": 0.75, "flowering": 1.1, "grain_filling": 0.85},
        "max_a": 100, "max_j": 180, "r_dark": 1.2,
        "season_days": 150, "planting_season": "Winter", "ndvi_initial": 0.3, "optimal_ph": (6.0, 7.5), "optimal_temp": (15, 25)
    },
    "Rice": {
        "harvest_index": 0.45, "max_biomass": 14, "max_leaf_area": 5.5,
        "gdd_stages": {"emergence": 90, "vegetative": 550, "flowering": 950, "grain_filling": 1300},
        "n_requirement": 140, "p_requirement": 50, "k_requirement": 100,
        "fc": 0.45, "pwp": 0.20, "swc_initial": 0.40, "root_depth": 0.8,
        "Kc_stages": {"sowing": 0.4, "emergence": 0.6, "vegetative": 0.9, "flowering": 1.2, "grain_filling": 1.0},
        "max_a": 130, "max_j": 210, "r_dark": 1.6,
        "season_days": 130, "planting_season": "Summer", "ndvi_initial": 0.3, "optimal_ph": (5.5, 7.0), "optimal_temp": (22, 32)
    }
}

# ==========================================
# 2. HELPER FUNCTIONS
# ==========================================

def generate_fallback_data(lat, lon):
    region = "Nile Delta" if lat > 30.5 else ("Middle Egypt" if lat > 29.0 else "Upper Egypt")
    base_temp = 24 if region == "Nile Delta" else (26 if region == "Middle Egypt" else 28)
    base_rain = 1.2 if region == "Nile Delta" else (0.8 if region == "Middle Egypt" else 0.4)
    soil_sand = 350 if region == "Nile Delta" else (400 if region == "Middle Egypt" else 450)
    soil_clay = 250 if region == "Nile Delta" else (200 if region == "Middle Egypt" else 180)
    
    return {
        'temperature': base_temp, 'precipitation': base_rain, 'humidity': 65, 'solar_radiation': 22000000,
        'ndvi': 0.52, 'sand': soil_sand, 'clay': soil_clay, 'soc': 140, 'ph': 68, 'cec': 230,
        'nitrogen': 0.15, 'phosphorus': 42, 'potassium': 175, 'latitude': lat, 'longitude': lon,
        'region': region
    }

def get_weather_forecast(lat, lon, days=150, base_t=25):
    try:
        weather = []
        for d in range(days):
            t = base_t + 8 * math.sin(2 * math.pi * d / 180) + np.random.normal(0, 2)
            r = np.random.exponential(3) if np.random.random() < (0.15 if d < 75 else 0.08) else 0.0
            h = max(30, min(80, 60 - (base_t - 25) * 2 + np.random.normal(0, 5)))
            rad = max(15000000, min(30000000, 20000000 + (base_t - 20) * 500000))
            weather.append({'temp': max(10, min(40, t)), 'rain': min(20, r), 'humidity': h, 'solar_rad': rad})
        return weather
    except: return []

def assess_soil(feats, params):
    sand, clay, soc = feats.get('sand', 400), feats.get('clay', 300), feats.get('soc', 150)
    tex_s = 1.0 * (0.6 if sand > 600 else 0.8 if sand > 500 else 0.7 if clay > 400 else 0.9 if clay > 300 else 1.0)
    tex_s *= (1.2 if soc > 180 else 0.8 if soc < 100 else 1.0)

    ph = feats.get('ph', 70) / 10.0
    opt = params['optimal_ph']
    ph_s = 1.0 if opt[0] <= ph <= opt[1] else max(0.3, 1.0 - (min(abs(ph - opt[0]), abs(ph - opt[1])) * 0.2))

    n, p, k = feats.get('nitrogen', 0) * 100, feats.get('phosphorus', 0), feats.get('potassium', 0)
    n_ad = min(1.0, (n * 20) / params['n_requirement'])
    p_ad = min(1.0, p / params['p_requirement'])
    k_ad = min(1.0, k / params['k_requirement'])
    
    nut_s = (n_ad * 0.4 + p_ad * 0.3 + k_ad * 0.3)
    qual = min(1.0, (tex_s * 0.3 + ph_s * 0.3 + nut_s * 0.4))
    
    # --- ADDED BACK: Region Feedback Logic ---
    lat = float(feats.get('latitude', 30.0))
    if lat > 30.5:
        feedback = "Nile Delta - Excellent for most crops"
    elif lat > 29.0:
        feedback = "Middle Egypt - Good for many crops"
    else:
        feedback = "Upper Egypt - Suitable with proper irrigation"

    return {
        'overall_quality': qual, 'texture_score': tex_s, 'ph_score': ph_s, 'nutrient_score': nut_s,
        'n_adequacy': n_ad, 'p_adequacy': p_ad, 'k_adequacy': k_ad, 'ph_value': ph,
        'region_feedback': feedback
    }

# ==========================================
# 3. PHYSICS & ODEs
# ==========================================

def penman_et0(t, rad, wind, hum):
    delta = 4098 * (0.6108 * np.exp((17.27 * t) / (t + 237.3))) / ((t + 237.3)**2)
    es = 0.6108 * np.exp((17.27 * t) / (t + 237.3))
    Rn = 0.77 * rad / 1000000 / 2.45
    return max(0, (0.408 * delta * Rn + 0.066 * (900 / (t + 273)) * wind * (es - (es * hum / 100))) / (delta + 0.066 * (1 + 0.34 * wind)))

def calc_lai(gdd, max_la, stages, ndvi):
    ndvi_f = max(0.7, min(1.3, ndvi / 0.3))
    if gdd < stages["flowering"]:
        return max(0.1, min(max_la * (1 / (1 + np.exp(-10 * (gdd / stages["flowering"] - 0.6)))) * ndvi_f, max_la * 1.3))
    return max(0.1, min(max_la * max(0.2, 1 - (gdd - stages["flowering"]) / (stages["grain_filling"] - stages["flowering"]) * 1.2) * ndvi_f, max_la * 1.3))

def water_ode(t, SWC, rain_f, irr_f, et0_f, params):
    rain, irr, et0 = rain_f(t), irr_f(t), et0_f(t)
    gdd = params['gdd_f'](t)
    stage = 'sowing'
    for k, v in params['gdd_stages'].items():
        if gdd >= v: stage = k
    kc = params['Kc_stages'].get(stage, 1.0)
    fc, pwp = params['fc'], params['pwp']
    drain = 8 * (SWC - fc) / (fc - pwp) if SWC > fc else 0.0
    ks = 1.0 if SWC >= fc else (0.0 if SWC <= pwp else max(0.0, (SWC - pwp) / ((fc - pwp) * 0.55)))
    return (rain + irr - (kc * et0 * ks) - drain) / (params['root_depth'] * 1000)

def biomass_ode(t, B, temp_f, swc_f, rad_f, gdd_f, params, soil_q):
    temp, swc, rad, gdd = temp_f(t), swc_f(t), rad_f(t), gdd_f(t)
    lai = calc_lai(gdd, params["max_leaf_area"], params["gdd_stages"], params["ndvi_initial"])
    ks = 1.0 if swc >= params['fc'] else (0.0 if swc <= params['pwp'] else max(0.0, (swc - params['pwp']) / ((params['fc'] - params['pwp']) * 0.35)))
    opt = params['optimal_temp']
    ts = 1.0
    if temp < opt[0]: ts = max(0.4, 1 - (opt[0] - temp) * 0.06)
    elif temp > opt[1]: ts = max(0.4, 1 - (temp - opt[1]) * 0.04)
    q10 = 2.0**((temp - 25) / 10)
    vcmax = params["max_a"] * q10
    jmax = params["max_j"] * q10
    aj = jmax * (1 - np.exp(-0.7 * (rad * 1000000 / 86400) / jmax))
    a_net = max(0, min(vcmax, aj) - (params["r_dark"] * (1.5**((temp - 25) / 10))))
    return min(0.00003 * a_net * lai * ks * ts * 0.5 * soil_q, 0.2)

# ==========================================
# 4. MAIN SIMULATION
# ==========================================

def simulate(crop, lat, lon, features=None):
    if crop not in CROP_PARAMS: return {"error": f"Crop {crop} not supported"}
    
    params = CROP_PARAMS[crop].copy()
    days = params['season_days']
    
    # Inputs
    feats = features if features else generate_fallback_data(lat, lon)
    soil_res = assess_soil(feats, params)
    soil_factor = soil_res['overall_quality']
    
    # Weather
    weather = get_weather_forecast(lat, lon, days, float(feats.get("temperature", 25)))
    temp = np.array([d['temp'] for d in weather])
    rain = np.array([d['rain'] for d in weather])
    rad = np.array([d['solar_rad'] for d in weather])
    hum = np.array([d['humidity'] for d in weather])
    wind = np.random.normal(2.0, 0.3, days)
    gdd = np.cumsum(np.maximum(0, temp - 10))
    
    # Irrigation
    irrigation = np.zeros(days)
    for i in range(days):
        if rain[i] < 2:
            stg_val = gdd[i]
            irr_amt = 8.0
            if stg_val > params['gdd_stages']['vegetative']: irr_amt = 12.0
            if stg_val > params['gdd_stages']['flowering']: irr_amt = 15.0
            if stg_val > params['gdd_stages']['grain_filling']: irr_amt = 10.0
            irrigation[i] = irr_amt

    # Solvers
    et0 = np.array([penman_et0(temp[i], rad[i], wind[i], hum[i]) for i in range(days)])
    t_span = np.arange(0, days, 1)
    params['gdd_f'] = interp1d(t_span, gdd, kind='linear', fill_value='extrapolate')
    params['ndvi_initial'] = float(feats.get("ndvi", 0.3))
    
    rain_f = interp1d(t_span, rain, kind='linear', fill_value='extrapolate')
    irr_f = interp1d(t_span, irrigation, kind='linear', fill_value='extrapolate')
    et0_f = interp1d(t_span, et0, kind='linear', fill_value='extrapolate')
    
    w_sol = solve_ivp(water_ode, [0, days-1], [params['swc_initial']], t_eval=t_span, args=(rain_f, irr_f, et0_f, params), method='RK45')
    swc = w_sol.y[0]
    
    temp_f = interp1d(t_span, temp, kind='linear', fill_value='extrapolate')
    swc_f = interp1d(t_span, swc, kind='linear', fill_value='extrapolate')
    rad_f = interp1d(t_span, rad, kind='linear', fill_value='extrapolate')
    
    b_sol = solve_ivp(biomass_ode, [0, days-1], [0.3], t_eval=t_span, args=(temp_f, swc_f, rad_f, params['gdd_f'], params, soil_factor), method='RK45')
    biomass = b_sol.y[0]

    # Analysis
    stage_names = []
    gs = params["gdd_stages"]
    for val in gdd:
        if val < gs["emergence"]: stage_names.append("Sowing")
        elif val < gs["vegetative"]: stage_names.append("Emergence")
        elif val < gs["flowering"]: stage_names.append("Vegetative")
        elif val < gs["grain_filling"]: stage_names.append("Flowering")
        else: stage_names.append("Grain Filling")

    stage_summary = {}
    unique_stages = sorted(list(set(stage_names)), key=lambda x: stage_names.index(x))
    
    rain_np = np.array(rain)
    irr_np = np.array(irrigation)
    temp_np = np.array(temp)
    bio_np = np.array(biomass)

    for stg in unique_stages:
        indices = [i for i, x in enumerate(stage_names) if x == stg]
        if indices:
            valid_indices = [i for i in indices if i < len(bio_np)]
            if valid_indices:
                stage_summary[stg.lower().replace(" ", "_")] = {
                    "days": len(valid_indices),
                    "irrigation": float(np.sum(irr_np[valid_indices])),
                    "rainfall": float(np.sum(rain_np[valid_indices])),
                    "avg_biomass": float(np.mean(bio_np[valid_indices])),
                    "avg_temp": float(np.mean(temp_np[valid_indices]))
                }

    # Stresses (Uncapped for report)
    fc, pwp = params['fc'], params['pwp']
    raw_growth = (fc - pwp) * 0.35
    water_stress_report = []
    
    for i in range(len(swc)):
        val = 0.0 if swc[i] <= params['pwp'] else max(0.0, (swc[i] - params['pwp']) / raw_growth)
        water_stress_report.append(float(val))

    water_stress_physics = [min(1.0, v) for v in water_stress_report]
    
    temp_stress = []
    opt = params['optimal_temp']
    for t_val in temp:
        ts = 1.0
        if t_val < opt[0]: ts = max(0.4, 1 - (opt[0] - t_val) * 0.06)
        elif t_val > opt[1]: ts = max(0.4, 1 - (t_val - opt[1]) * 0.04)
        temp_stress.append(float(ts))

    avg_w_phys = np.mean(water_stress_physics)
    avg_t = np.mean(temp_stress)
    base_yield = biomass[-1] * params["harvest_index"]
    water_factor = 1 - ((1 - avg_w_phys) * 0.7)
    pred_yield = base_yield * soil_factor * water_factor * avg_t

    n_def = max(0, params['n_requirement'] - soil_res['n_adequacy'] * params['n_requirement'])
    p_def = max(0, params['p_requirement'] - soil_res['p_adequacy'] * params['p_requirement'])
    k_def = max(0, params['k_requirement'] - soil_res['k_adequacy'] * params['k_requirement'])

    return {
        "crop": crop,
        "predicted_yield": float(pred_yield),
        "potential_yield": float(base_yield),
        "yield_efficiency": float((pred_yield / base_yield) * 100) if base_yield > 0 else 0,
        "biomass": biomass.tolist(),
        "swc": swc.tolist(),
        "et0_daily": et0.tolist(),
        "water_stress": water_stress_report,
        "temperature_stress": temp_stress,
        "days": list(range(1, days + 1)),
        "growth_stages": stage_names,
        "stage_summary": stage_summary,
        "soil_assessment": soil_res,
        "irrigation_schedule": irrigation.tolist(),
        "total_irrigation": float(np.sum(irrigation)),
        "total_rainfall": float(np.sum(rain)),
        "fertilizer_needs": {
            "nitrogen": float(n_def),
            "phosphorus": float(p_def),
            "potassium": float(k_def)
        },
        "stress_factors": {
            "soil": float(soil_factor),
            "water": float(np.mean(water_stress_report)),
            "temperature": float(avg_t)
        },
        "gdd": gdd.tolist(), 
        "api_inputs": feats,
        "timestamp": datetime.now().isoformat(),
        "version": "5.0-with-region-feedback",
        "calibration": "egypt_specific"
    }

if __name__ == "__main__":
    try:
        if len(sys.argv) > 1: input_json = sys.argv[1]
        else: input_json = sys.stdin.read()
        
        if not input_json: raise ValueError("No input data received")

        data = json.loads(input_json)
        crop = data.get("cropName")
        lat = float(data.get("latitude", 30.0))
        lon = float(data.get("longitude", 31.0))
        feats = data.get("features", {}) 

        output = simulate(crop, lat, lon, feats)
        print(json.dumps(output))
        
    except Exception as e:
        print(json.dumps({"error": str(e)}))