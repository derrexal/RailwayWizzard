from dataclasses import dataclass, field
from typing import Dict, List, Optional

from bot.data.CarType import CarType


NOTIFICATION_STATE_KEY = "notification_state"


def build_default_car_type_selection() -> Dict[str, bool]:
    return {car_type.name: car_type.is_sitting for car_type in CarType}


@dataclass
class NotificationConversationState:
    stage: Optional[int] = None
    popular_cities: List[str] = field(default_factory=list)
    departure_station: Optional[str] = None
    departure_express_code: Optional[int] = None
    arrival_station: Optional[str] = None
    arrival_express_code: Optional[int] = None
    departure_date: Optional[str] = None
    departure_date_text: Optional[str] = None
    departure_time: Optional[str] = None
    user_id: Optional[int] = None
    car_types: List[int] = field(default_factory=list)
    number_seats: Optional[int] = None
    car_type_selection: Dict[str, bool] = field(
        default_factory=build_default_car_type_selection
    )
