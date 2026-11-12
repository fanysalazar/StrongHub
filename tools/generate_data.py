"""Generates the JSON data files for StrongHub (run from the repository root).

Usage:  python tools/generate_data.py
Output: data/users.json, trainers.json, routines.json, visits.json, measurements.json
"""
import json
import os
import random
import datetime as dt

random.seed(2026)  # fixed seed -> same data every run
TODAY = dt.date(2026, 11, 11)
FIRST = ["Ana", "Luis", "Marta", "Diego", "Sofia", "Carlos", "Elena", "Pablo", "Lucia", "Jorge",
         "Valeria", "Andres", "Camila", "Mateo", "Daniela", "Esteban", "Paula", "Ricardo", "Karla", "Sergio"]
LAST = ["Mora", "Vargas", "Rojas", "Solano", "Chaves", "Jimenez", "Araya", "Campos", "Salas", "Quiros",
        "Murillo", "Herrera", "Castro", "Alvarado", "Vega", "Brenes", "Calderon", "Soto", "Porras", "Zuniga"]
CLASSES = ["Yoga", "Open gym", "Crossfit", "Zumba", "Spinning"]
EXERCISES = ["Squat", "Bench press", "Deadlift", "Pull-up", "Shoulder press", "Barbell row", "Lunge", "Plank"]
N_USERS, N_TRAINERS = 50, 45


def day(days_back):
    return (TODAY - dt.timedelta(days=days_back)).isoformat()


def full_name():
    return f"{random.choice(FIRST)} {random.choice(LAST)}"


users = []
for i in range(1, N_USERS + 1):
    height = round(random.uniform(1.50, 1.95), 2)
    weight = round(random.uniform(50, 110), 1)
    users.append({
        "id": i, "name": full_name(), "username": f"user{i}", "age": random.randint(16, 60),
        "weightKg": weight, "heightM": height, "bmi": round(weight / height ** 2, 2),
        "trainerId": random.randint(1, N_TRAINERS),
        "goalBodyFatPercent": random.randint(10, 22),
        "goalMuscleKg": round(random.uniform(30, 50), 1),
    })

trainers = [{"id": i, "name": full_name(), "username": f"trainer{i}",
             "type": random.choice(["Personal", "Gym"])} for i in range(1, N_TRAINERS + 1)]

routines, rid = [], 1
for u in users:
    for k in range(random.randint(5, 7)):
        start_back = 150 - k * 22 + random.randint(-3, 3)   # spread over time; the newest ones are recent
        start = TODAY - dt.timedelta(days=start_back)
        routines.append({
            "id": rid, "userId": u["id"], "trainerId": u["trainerId"], "name": f"Routine {k + 1}",
            "startDate": start.isoformat(), "endDate": (start + dt.timedelta(days=28)).isoformat(),
            "exercises": [{"name": e, "sets": random.randint(3, 5), "reps": random.randint(6, 15),
                           "weightKg": random.randint(10, 100)} for e in random.sample(EXERCISES, 3)],
        })
        rid += 1

# Some users attend a lot, others very little (useful for the top-10 rankings and the shame badges)
activity = {u["id"]: random.choice([0.2, 0.5, 1, 1, 2, 3]) for u in users}
visits, vid = [], 1
for u in users:
    for _ in range(int(random.randint(4, 12) * activity[u["id"]]) + 1):
        visits.append({"id": vid, "userId": u["id"], "gymClass": random.choice(CLASSES),
                       "date": day(random.randint(0, 75))})
        vid += 1
visits.sort(key=lambda v: v["date"])
for n, v in enumerate(visits, 1):
    v["id"] = n

measurements, mid = [], 1
for u in users:
    w, fat, muscle = u["weightKg"], random.uniform(15, 30), random.uniform(28, 45)
    for back in (84, 56, 28, 3):
        trend = random.uniform(-1.5, 0.8)
        w, fat, muscle = round(w + trend, 1), round(fat + trend * 0.4, 1), round(muscle - trend * 0.3, 1)
        measurements.append({"id": mid, "userId": u["id"], "date": day(back),
                             "weightKg": w, "bodyFatPercent": fat, "muscleKg": muscle})
        mid += 1

os.makedirs("data", exist_ok=True)
files = {"users": users, "trainers": trainers, "routines": routines,
         "visits": visits, "measurements": measurements}
for name, rows in files.items():
    assert len(rows) >= 40, f"{name} has fewer than 40 records"
    with open(f"data/{name}.json", "w", encoding="utf-8") as f:
        json.dump(rows, f, indent=2, ensure_ascii=False)
    print(f"{name}.json: {len(rows)} records")
